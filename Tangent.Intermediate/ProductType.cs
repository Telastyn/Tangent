using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ProductType : TangentType, HasGenericParameters
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

            return BoundGenericType.For(this, bindings);
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) { return Enumerable.Empty<ParameterDeclaration>(); }
            alreadyProcessed.Add(this);

            return GenericParameters;
        }

        IEnumerable<ParameterDeclaration> HasGenericParameters.GenericParameters
        {
            get
            {
                return GenericParameters;
            }
        }
    }
}
