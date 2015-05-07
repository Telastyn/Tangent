using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class DelegatingTypeLookup : ITypeLookup
    {
        private readonly ITypeCompiler typeCompiler;
        private readonly IEnumerable<TypeDeclaration> declaredTypes;
        private readonly Dictionary<TangentType, Type> lookup = new Dictionary<TangentType, Type>();

        public DelegatingTypeLookup(ITypeCompiler typeCompiler, IEnumerable<TypeDeclaration> declaredTypes)
        {
            this.typeCompiler = typeCompiler;
            this.declaredTypes = declaredTypes;
        }

        public Type this[TangentType t]
        {
            get
            {
                if (!lookup.ContainsKey(t)) {
                    PopulateLookupWith(t);
                }

                return lookup[t];
            }
        }

        private void PopulateLookupWith(TangentType t)
        {
            switch (t.ImplementationType) {
                case KindOfType.Enum:
                case KindOfType.Product:
                    // This should already be declared in our types.
                    var result = declaredTypes.FirstOrDefault(td => td.Returns == t);
                    if (result == null) {
                        throw new ApplicationException("Got TypeLookup request for a type that wasn't declared?");
                    }

                    var type = typeCompiler.Compile(result, placeholder => lookup.Add(result.Returns, placeholder), tt => this[tt]);
                    if (lookup.ContainsKey(result.Returns)) {
                        lookup[result.Returns] = type;
                    } else {
                        lookup.Add(result.Returns, type);
                    }
                    return;

                case KindOfType.Lazy:
                    // The target of the type constructor needs to be already declared in our types.
                    var lazyType = t as LazyType;
                    var target = this[lazyType.Type];

                    if (target == typeof(void)) {
                        lookup.Add(t, typeof(Action));
                    } else {
                        lookup.Add(t, typeof(Func<>).MakeGenericType(target));
                    }
                    return;

                case KindOfType.SingleValue:
                    throw new NotImplementedException("Something is asking for the type of a SingleValueType. We should never get here.");

                case KindOfType.Builtin:
                    lookup.Add(TangentType.Void, typeof(void));
                    lookup.Add(TangentType.String, typeof(string));
                    return;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
