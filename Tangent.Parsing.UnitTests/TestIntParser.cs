using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    internal class TestIntParser : Parser<int>
    {
        public static readonly Parser<int> Common = new TestIntParser();

        public override ResultOrParseError<int> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Identifier != TokenIdentifier.IntegerConstant) {
                consumed = 0;
                return new ResultOrParseError<int>(new ExpectedTokenParseError(TokenIdentifier.IntegerConstant, first));
            }

            consumed = 1;
            return int.Parse(first.Value);
        }
    }
}
