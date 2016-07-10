using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DotNetType:TangentType
    {
        public readonly Type MappedType;

        private DotNetType(Type target) : base(KindOfType.Builtin)
        {
            MappedType = target;
        }

        private static readonly ConcurrentDictionary<Type, TangentType> cache = new ConcurrentDictionary<Type, TangentType>();
        public static TangentType For(Type t)
        {
            // TODO: handle bool as an enum type.
            if(t == typeof(bool)) {
                return TangentType.Bool;
            }

            if (t.IsEnum) {
                return cache.GetOrAdd(t, x => DotNetEnumType.For(t));
            }

            // TODO: interfaces.
            if (t.IsInterface) {
                return null;
            }

            // TODO: reference type -> sum type+null
            if (!t.IsValueType) {
                return null;
            }

            // TODO: handle generics.
            if(t.IsGenericTypeDefinition || t.IsGenericType) {
                return null;
            }

            return cache.GetOrAdd(t, x => new DotNetType(t));
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType RebindInferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override string ToString()
        {
            return ".NET " + MappedType.FullName;
        }
    }
}
