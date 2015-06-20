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
    }
}
