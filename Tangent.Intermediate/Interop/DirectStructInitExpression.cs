using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectStructInitExpression:Expression
    {
        public readonly Type TargetStruct;
        public DirectStructInitExpression(Type target) : base(null)
        {
            TargetStruct = target;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return DotNetType.For(TargetStruct);
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.DirectStructInit;
            }
        }
    }
}
