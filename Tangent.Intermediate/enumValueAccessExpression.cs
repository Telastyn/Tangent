using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumValueAccessExpression : Expression
    {
        public readonly SingleValueType EnumValue;
        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.EnumValueAccess; }
        }

        public EnumValueAccessExpression(SingleValueType svt)
        {
            EnumValue = svt;
        }
    }
}
