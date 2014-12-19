using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Tokenization;

namespace Tangent.Parsing.Errors
{
    public class ExpectedTokenParseError : ParseError
    {
        public readonly TokenIdentifier Expected;
        public readonly Token FoundInstead;

        public ExpectedTokenParseError(TokenIdentifier expected, Token foundInstead)
        {
            Expected = expected;
            FoundInstead = foundInstead;
        }
    }
}
