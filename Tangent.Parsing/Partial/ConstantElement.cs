using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public abstract class ConstantElement : PartialElement
    {
        internal abstract Expression TypelessExpression { get; }
        public ConstantElement() : base(ElementType.Constant) { }
    }

    public class ConstantElement<T> : ConstantElement
    {
        public readonly ConstantExpression<T> Expression;
        public ConstantElement(ConstantExpression<T> expr)
        {
            this.Expression = expr;
        }

        internal override Expression TypelessExpression
        {
            get { return Expression; }
        }
    }
}
