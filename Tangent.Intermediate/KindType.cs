using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class KindType : TangentType
    {
        public readonly TangentType KindOf;
        public KindType(TangentType kindof)
            : base(KindOfType.Kind)
        {
            KindOf = kindof;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) { return true; }
            var kindOther = other as KindType;
            if (kindOther == null) { return false; }
            return this.KindOf.CompatibilityMatches(kindOther.KindOf, necessaryTypeInferences);
        }
    }
}
