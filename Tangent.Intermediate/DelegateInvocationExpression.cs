using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class DelegateInvocationExpression : Expression
    {
        public readonly Expression Delegate;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.DelegateInvocation; }
        }

        public DelegateInvocationExpression(Expression d)
            : base(d.SourceInfo)
        {
            Delegate = d;
        }

        public override TangentType EffectiveType
        {
            get
            {
                switch (Delegate.NodeType)
                {
                    case ExpressionNodeType.ParameterAccess:
                        var parameter = Delegate as ParameterAccessExpression;
                        var lazyType = parameter.Parameter.Returns as LazyType;
                        return lazyType.Type;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
