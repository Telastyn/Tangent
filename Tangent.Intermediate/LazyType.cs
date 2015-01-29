using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LazyType : TangentType
    {
        public readonly TangentType Type;
        public LazyType(TangentType t)
            : base(KindOfType.Lazy)
        {
            Type = t;
        }
    }
}
