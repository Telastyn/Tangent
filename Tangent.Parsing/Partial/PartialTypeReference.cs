using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialTypeReference : PlaceholderType
    {
        public readonly IEnumerable<IdentifierExpression> Identifiers;
        public readonly IEnumerable<PartialParameterDeclaration> GenericArgumentPlaceholders;
        public TangentType ResolvedType = null;

        public PartialTypeReference(IEnumerable<IdentifierExpression> parts, IEnumerable<PartialParameterDeclaration> genericArgs)
        {
            Identifiers = new List<IdentifierExpression>(parts);
            GenericArgumentPlaceholders = genericArgs;
        }
    }
}
