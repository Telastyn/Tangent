using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialPhrasePart
    {
        public readonly IdentifierExpression Identifier;
        public readonly PartialParameterDeclaration Parameter;
        public bool IsIdentifier { get { return Identifier != null; } }

        public PartialPhrasePart(IdentifierExpression id)
        {
            Identifier = id;
        }

        public PartialPhrasePart(PartialParameterDeclaration decl)
        {
            Parameter = decl;
        }

        public static implicit operator PartialPhrasePart(IdentifierExpression id)
        {
            return new PartialPhrasePart(id);
        }

        public static implicit operator PartialPhrasePart(PartialParameterDeclaration decl)
        {
            return new PartialPhrasePart(decl);
        }
    }
}
