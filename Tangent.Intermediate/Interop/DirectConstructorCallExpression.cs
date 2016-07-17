using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectConstructorCallExpression: Expression
    {
        public readonly ConstructorInfo Constructor;
        public readonly IEnumerable<Expression> Arguments;

        public DirectConstructorCallExpression(ConstructorInfo ctor, IEnumerable<Expression> args) : base(null)
        {
            Constructor = ctor;
            Arguments = args;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return DotNetType.For(Constructor.DeclaringType);
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.DirectConstructorCall;
            }
        }
    }
}
