using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class BoundGenericType : TangentType
    {
        public readonly TypeDeclaration GenericTypeDeclatation;
        public readonly IEnumerable<TangentType> TypeArguments;
        public TangentType GetBoundTypeFor(ParameterDeclaration genericParameter)
        {
            return BindingStore[genericParameter];
        }

        public TangentType GetBoundTypeFor(GenericArgumentReferenceType refType)
        {
            return GetBoundTypeFor(refType.GenericParameter);
        }

        private Dictionary<ParameterDeclaration, TangentType> bindingStore = null;
        private Dictionary<ParameterDeclaration, TangentType> BindingStore
        {
            get
            {
                bindingStore = bindingStore ?? GenericTypeDeclatation.Takes.Where(pp => !pp.IsIdentifier).Zip(TypeArguments, (pp, ta) => new KeyValuePair<ParameterDeclaration, TangentType>(pp.Parameter, ta)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return bindingStore;
            }
        }

        private BoundGenericType(TypeDeclaration genericTypeDecl, IEnumerable<TangentType> arguments)
            : base(KindOfType.BoundGeneric)
        {
            if (!genericTypeDecl.IsGeneric) { throw new InvalidOperationException(string.Format("Specified generic type '{0}' isn't generic.", genericTypeDecl)); }
            if (genericTypeDecl.Takes.Count(pp => !pp.IsIdentifier) != arguments.Count()) { throw new InvalidOperationException(string.Format("Specified type argument count does not match specified generic type.")); }

            GenericTypeDeclatation = genericTypeDecl;
            TypeArguments = arguments;
        }

        private static readonly Dictionary<TypeDeclaration, Dictionary<IEnumerable<TangentType>, BoundGenericType>> cache = new Dictionary<TypeDeclaration, Dictionary<IEnumerable<TangentType>, BoundGenericType>>();

        public static BoundGenericType For(TypeDeclaration genericTypeDecl, IEnumerable<TangentType> arguments)
        {
            lock (cache) {
                if (!cache.ContainsKey(genericTypeDecl)) {
                    cache.Add(genericTypeDecl, new Dictionary<IEnumerable<TangentType>, BoundGenericType>());
                }

                foreach (var concreteVersion in cache[genericTypeDecl]) {
                    if (arguments.SequenceEqual(concreteVersion.Key)) {
                        return concreteVersion.Value;
                    }
                }

                var result = new BoundGenericType(genericTypeDecl, arguments);
                cache[genericTypeDecl].Add(arguments, result);
                return result;
            }
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) { return true; }
            var boundOther = other as BoundGenericType;
            if (boundOther == null) { return false; }
            if (this.GenericTypeDeclatation != boundOther.GenericTypeDeclatation) { return false; }
            var nested = new Dictionary<ParameterDeclaration, TangentType>();
            foreach (var pair in TypeArguments.Zip(boundOther.TypeArguments, (a, b) => Tuple.Create(a, b))) {
                if (!pair.Item1.CompatibilityMatches(pair.Item2, nested)) {
                    return false;
                }
            }

            foreach (var inference in nested) {
                necessaryTypeInferences.Add(inference.Key, inference.Value);
            }

            return true;
        }
    }
}
