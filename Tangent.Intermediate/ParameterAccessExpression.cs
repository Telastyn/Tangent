using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ParameterAccessExpression : Expression
    {
        public readonly ParameterDeclaration Parameter;
        public readonly IEnumerable<Expression> Arguments;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.ParameterAccess; }
        }

        public override TangentType EffectiveType
        {
            get { return Parameter.Returns; }
        }

        public ParameterAccessExpression(ParameterDeclaration decl, IEnumerable<Expression> arguments, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Parameter = decl;
            Arguments = arguments;
        }

        public override string ToString()
        {
            return string.Format("param '{0}'", string.Join(" ", Parameter.Takes));
        }
    }
}
