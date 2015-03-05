using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ParameterAccessExpression : Expression
    {
        public readonly ParameterDeclaration Parameter;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.ParameterAccess; }
        }

        public ParameterAccessExpression(ParameterDeclaration decl, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Parameter = decl;
        }
    }
}
