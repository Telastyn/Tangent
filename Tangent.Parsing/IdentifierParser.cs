using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class IdentifierParser : Parser<IdentifierExpression>
    {
        public static readonly IdentifierParser Common = new IdentifierParser();

        public override ResultOrParseError<IdentifierExpression> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Identifier != TokenIdentifier.Identifier) {
                consumed = 0;
                return new ResultOrParseError<IdentifierExpression>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
            }

            consumed = 1;
            return new IdentifierExpression(first.Value, first.SourceInfo);
        }
    }
}
