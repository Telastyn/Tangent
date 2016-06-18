﻿using System;
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
        private IEnumerable<ParameterDeclaration> genericElements = null;

        public ProductType(IEnumerable<PhrasePart> dataConstructorParts, IEnumerable<Field> fields)
            : base(KindOfType.Product)
        {
            this.DataConstructorParts = new List<PhrasePart>(dataConstructorParts);
            this.Fields = new List<Field>(fields);
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

        public override TangentType RebindInferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return ResolveGenericReferences(mapping);
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) { return Enumerable.Empty<ParameterDeclaration>(); }
            alreadyProcessed.Add(this);

            // For product types, generic references are also inferences.
            genericElements = genericElements ?? DataConstructorParts.Where(pp => !pp.IsIdentifier).SelectMany(pp => pp.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference, alreadyProcessed));
            return genericElements;
        }
    }
}
