using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class OptionalParser<T> : Parser<T>
    {
        private readonly Parser<T> internalParser;

        public OptionalParser(Parser<T> wrapped)
        {
            internalParser = wrapped;
        }

        public override ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var result = internalParser.Parse(tokens, out consumed);
            if (result.Success) {
                return result;
            }

            return new ResultOrParseError<T>(default(T));
        }
    }
}
