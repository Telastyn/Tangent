using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ProductType : TangentType
    {
        public readonly List<PhrasePart> DataConstructorParts;
        private IEnumerable<ParameterDeclaration> genericElements = null;

        public ProductType(IEnumerable<PhrasePart> dataConstructorParts)
            : base(KindOfType.Product)
        {
            this.DataConstructorParts = new List<PhrasePart>(dataConstructorParts);
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            // Doing this lazily since we don't have generic info at ctor time.
            genericElements = genericElements ?? ContainedGenericReferences(GenericTie.Inference);
            if (!genericElements.Any()) {
                return this;
            }

            var bindings = genericElements.Select(pd => mapping(pd)).ToList();
           
            return BoundGenericProductType.For(this, bindings);
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            // For product types, generic references are also inferences.
            genericElements = genericElements ?? DataConstructorParts.Where(pp => !pp.IsIdentifier).SelectMany(pp => pp.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference));
            return genericElements;
        }
    }
}
