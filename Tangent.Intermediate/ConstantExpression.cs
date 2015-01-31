using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ConstantExpression : Expression
    {
        public readonly TangentType EffectiveType;
        public readonly object Value;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Constant; }
        }

        protected ConstantExpression(TangentType type, object value)
        {
            this.EffectiveType = type;
            this.Value = value;
        }
    }

    public class ConstantExpression<T> : ConstantExpression
    {
        public T TypedValue { get { return (T)Value; } }

        public ConstantExpression(TangentType type, T value): base (type, value)
        {
        }
    }
}
