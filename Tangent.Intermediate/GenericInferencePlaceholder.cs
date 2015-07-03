using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericInferencePlaceholder : TangentType
    {
        public readonly ParameterDeclaration GenericArgument;
        private GenericInferencePlaceholder(ParameterDeclaration genericArg)
            : base(KindOfType.InferencePoint)
        {
            GenericArgument = genericArg;
        }

        private static readonly ConcurrentDictionary<ParameterDeclaration, GenericInferencePlaceholder> cache = new ConcurrentDictionary<ParameterDeclaration, GenericInferencePlaceholder>();

        public static GenericInferencePlaceholder For(ParameterDeclaration genericArg)
        {
            return cache.GetOrAdd(genericArg, pd => new GenericInferencePlaceholder(pd));
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            // TODO: verify generic constraint.
            if (necessaryTypeInferences.ContainsKey(GenericArgument)) {
                if (necessaryTypeInferences[GenericArgument] != other) {
                    // Some inference mismatch. We should probably try to provide better errors.
                    //  Should probably also work to intersect the inferences. For now, just fail.
                    return false;
                }

                return true;
            }

            necessaryTypeInferences.Add(GenericArgument, other);
            return true;
        }
    }
}
