using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericArgumentReferenceType : TangentType
    {
        public readonly ParameterDeclaration GenericParameter;
        private GenericArgumentReferenceType(ParameterDeclaration genericParam)
            : base(KindOfType.GenericReference)
        {
            GenericParameter = genericParam;
        }

        private static ConcurrentDictionary<ParameterDeclaration, GenericArgumentReferenceType> cache = new ConcurrentDictionary<ParameterDeclaration, GenericArgumentReferenceType>();

        public static GenericArgumentReferenceType For(ParameterDeclaration decl)
        {
            return cache.GetOrAdd(decl, pd => new GenericArgumentReferenceType(pd));
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) {
                return true;
            }

            if (necessaryTypeInferences.ContainsKey(this.GenericParameter)) {
                return necessaryTypeInferences[this.GenericParameter].CompatibilityMatches(other, necessaryTypeInferences);
            }

            var otherInference = other as GenericInferencePlaceholder;
            if (otherInference != null) {
                return this.GenericParameter == otherInference.GenericArgument;
            }

            // TODO: test constraints.
            // TODO: This isn't really an inference, but we need to set something so that later inferences fail if there's a mismatch.
            necessaryTypeInferences.Add(GenericParameter, other);
            return true;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return mapping(this.GenericParameter);
        }

        public override TangentType RebindInferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            if (tie == GenericTie.Reference) { yield return this.GenericParameter; }
        }
    }
}
