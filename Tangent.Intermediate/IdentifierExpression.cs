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

        public override TangentType EffectiveType
        {
            get { return null; }
        }

        public IdentifierExpression(Identifier identifier, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Identifier = identifier;
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }
    }
}
