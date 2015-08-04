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
        //private IEnumerable<ParameterDeclaration> genericElements = null;
        //private readonly Dictionary<List<TangentType>, ProductType> concreteTypes = new Dictionary<List<TangentType>, ProductType>();

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
            return this;
            //genericElements = genericElements ?? ContainedGenericReferences(GenericTie.Inference).Concat(ContainedGenericReferences(GenericTie.Reference));
            //if (!genericElements.Any()) {
            //    return this;
            //}

            //var bindings = genericElements.Select(pd => mapping(pd)).ToList();
            //foreach (var entry in concreteTypes) {
            //    if (entry.Key.Count == bindings.Count && entry.Key.SequenceEqual(bindings)) {
            //        return entry.Value;
            //    }
            //}

            //var concreteType = new ProductType(DataConstructorParts.Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns.ResolveGenericReferences(mapping)))));
            //concreteTypes.Add(bindings, concreteType);
            //return concreteType;
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            return DataConstructorParts.Where(pp => !pp.IsIdentifier).SelectMany(pp => pp.Parameter.Returns.ContainedGenericReferences(tie));
        }
    }
}
