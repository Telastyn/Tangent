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

        public override TangentType EffectiveType
        {
            get { return EnumValue; }
        }

        public EnumValueAccessExpression(SingleValueType svt, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            EnumValue = svt;
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }
    }
}
