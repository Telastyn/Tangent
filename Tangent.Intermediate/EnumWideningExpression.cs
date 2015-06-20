using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumWideningExpression : Expression
    {
        public readonly EnumValueAccessExpression EnumAccess;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.EnumWidening; }
        }

        public override TangentType EffectiveType
        {
            get { return EnumAccess.EnumValue.ValueType; }
        }

        public EnumWideningExpression(EnumValueAccessExpression expr)
            : base(expr.SourceInfo)
        {
            this.EnumAccess = expr;
        }
    }
}
