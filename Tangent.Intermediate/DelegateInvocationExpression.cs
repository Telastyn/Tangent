using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class DelegateInvocationExpression : Expression
    {
        public readonly IEnumerable<Expression> Arguments;
        public readonly Expression DelegateAccess;

        public DelegateInvocationExpression(Expression delegateAccess, IEnumerable<Expression> arguments, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            DelegateAccess = delegateAccess;
            Arguments = arguments;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.DelegateInvocation; }
        }

        public DelegateType DelegateType
        {
            get
            {
                var paramAccess = DelegateAccess as ParameterAccessExpression;

                if (paramAccess != null) {
                    return paramAccess.Parameter.RequiredArgumentType as DelegateType;
                } else {
                    return DelegateAccess.EffectiveType as DelegateType;
                }
            }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return DelegateType.Returns;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = Arguments.Select(expr => expr.ReplaceParameterAccesses(mapping));
            var newAccess = DelegateAccess.ReplaceParameterAccesses(mapping); 

            if(Arguments.SequenceEqual(newbs) && DelegateAccess == newAccess) {
                return this;
            }

            return new DelegateInvocationExpression(newAccess, newbs, SourceInfo);
        }
    }
}
