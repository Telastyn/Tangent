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
        public readonly List<Field> Fields;
        public readonly List<ParameterDeclaration> GenericParameters;

        public ProductType(IEnumerable<PhrasePart> dataConstructorParts, IEnumerable<ParameterDeclaration> genericTypeParameters, IEnumerable<Field> fields)
            : base(KindOfType.Product)
        {
            this.DataConstructorParts = new List<PhrasePart>(dataConstructorParts);
            this.Fields = new List<Field>(fields);
            this.GenericParameters = new List<ParameterDeclaration>(genericTypeParameters);
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            // Doing this lazily since we don't have generic info at ctor time.
            if (!GenericParameters.Any()) {
                return this;
            }

            var bindings = GenericParameters.Select(pd => mapping(pd)).ToList();

            return BoundGenericProductType.For(this, bindings);
        }

        public override TangentType RebindInferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return ResolveGenericReferences(mapping);
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) { return Enumerable.Empty<ParameterDeclaration>(); }
            alreadyProcessed.Add(this);

            if (tie == GenericTie.Inference) {
                return DataConstructorParts.Where(pp => !pp.IsIdentifier).SelectMany(pp => pp.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference, alreadyProcessed));
            } else if (tie == GenericTie.Reference) {
                return GenericParameters;
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
