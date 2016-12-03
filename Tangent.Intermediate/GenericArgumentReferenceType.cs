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

            var othersvt = other as SingleValueType;
            if (othersvt != null) {
                return CompatibilityMatches(othersvt.ValueType, necessaryTypeInferences);
            }

            // TODO: verify generic constraint.
            if (necessaryTypeInferences.ContainsKey(GenericParameter)) {
                if (necessaryTypeInferences[GenericParameter] != other) {
                    // Some inference mismatch. We should probably try to provide better errors.
                    //  Should probably also work to intersect the inferences. For now, just fail.
                    return false;
                }

                return true;
            }

            necessaryTypeInferences.Add(GenericParameter, other);
            return true;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return mapping(this.GenericParameter);
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            yield return this.GenericParameter;
        }
    }
}
