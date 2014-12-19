using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class TypeAccessExpression : Expression
    {
        public readonly TangentType TypeConstant;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.TypeAccess; }
        }

        public TypeAccessExpression(TangentType type)
        {
            TypeConstant = type;
        }
    }
}
