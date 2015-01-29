using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumType : TangentType
    {
        public readonly IEnumerable<Identifier> Values;

        public EnumType(IEnumerable<Identifier> values)
            : base(KindOfType.Enum)
        {
            Values = values;
        }
    }
}
