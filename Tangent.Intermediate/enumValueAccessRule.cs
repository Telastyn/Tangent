using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumValueAccessRule : ReductionRule<Identifier, SingleValueType>
    {
        public EnumValueAccessRule(EnumType type, Identifier value) : base(new[] { value }, type.SingleValueTypeFor(value)) { }
    }
}
