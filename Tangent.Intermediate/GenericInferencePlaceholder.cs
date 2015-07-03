using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericInferencePlaceholder : TangentType
    {
        public readonly ParameterDeclaration GenericArgument;
        public GenericInferencePlaceholder(ParameterDeclaration genericArg)
            : base(KindOfType.InferencePoint)
        {
            GenericArgument = genericArg;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            // TODO: verify generic constraint.
            necessaryTypeInferences.Add(GenericArgument, other);
            return true;
        }
    }
}
