using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class IdentifierExpression : Expression
    {
        public readonly Identifier Identifier;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Identifier; }
        }

        public IdentifierExpression(Identifier identifier)
        {
            Identifier = identifier;
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }
    }
}
