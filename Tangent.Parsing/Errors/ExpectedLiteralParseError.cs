using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Tokenization;

namespace Tangent.Parsing.Errors
{
    public class ExpectedLiteralParseError : ParseError
    {
        public readonly string Expected;
        public readonly Token Found;

        public ExpectedLiteralParseError(string expected, Token found)
        {
            Expected = expected;
            Found = found;
        }

        public override string ToString()
        {
            return string.Format("Expected '{0}', but found '{1}' at {2}", Expected, Found.Value, Found.SourceInfo);
        }
    }
}
