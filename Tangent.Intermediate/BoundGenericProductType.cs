using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class BoundGenericProductType : TangentType
    {
        public readonly ProductType GenericProductType;
        public readonly IEnumerable<TangentType> TypeArguments;

        private static readonly List<BoundGenericProductType> concreteTypes = new List<BoundGenericProductType>();

        private BoundGenericProductType(ProductType generic, IEnumerable<TangentType> arguments)
            : base(KindOfType.BoundGenericProduct)
        {
            GenericProductType = generic;
            TypeArguments = arguments;
        }

        public static BoundGenericProductType For(ProductType generic, IEnumerable<TangentType> arguments)
        {
            if (arguments.Count() != generic.ContainedGenericReferences(GenericTie.Inference).Count()) { throw new InvalidOperationException(); }
            var result = concreteTypes.FirstOrDefault(t => t.GenericProductType == generic && t.TypeArguments.SequenceEqual(arguments));
            if (result != null) {
                return result;
            }

            result = new BoundGenericProductType(generic, arguments);
            concreteTypes.Add(result);
            return result;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            var boundOther = other as BoundGenericProductType;
            if (boundOther == null) { return false; }
            if (boundOther.GenericProductType != this.GenericProductType) { return false; }
            foreach (var pair in this.TypeArguments.Zip(boundOther.TypeArguments, (a, b) => Tuple.Create(a, b))) {
                if (!pair.Item1.CompatibilityMatches(pair.Item2, necessaryTypeInferences)) {
                    return false;
                }
            }

            return true;
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            return TypeArguments.SelectMany(t => t.ContainedGenericReferences(tie));
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return BoundGenericProductType.For(this.GenericProductType, this.TypeArguments.Select(t => t.ResolveGenericReferences(mapping)));
        }
    }
}
