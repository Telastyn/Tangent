using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TypeConstant : TangentType
    {
        public readonly TangentType Value;
        public TypeConstant(TangentType value)
            : base(KindOfType.TypeConstant)
        {
            Value = value;
        }
    }
}
