using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ConstantExpression : Expression
    {
        private TangentType effectiveType;
        public override TangentType EffectiveType
        {
            get
            {
                return effectiveType;
            }
        }

        public readonly object Value;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Constant; }
        }

        protected ConstantExpression(TangentType type, object value, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            this.effectiveType = type;
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }
    }

    public class ConstantExpression<T> : ConstantExpression
    {
        public T TypedValue { get { return (T)Value; } }

        public ConstantExpression(TangentType type, T value, LineColumnRange sourceInfo)
            : base(type, value, sourceInfo)
        {
        }
    }
}
