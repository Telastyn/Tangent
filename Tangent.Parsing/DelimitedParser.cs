using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class DelimitedParser<T, U> : Parser<IEnumerable<T>>
    {
        private readonly Parser<T> meaningfulParser;
        private readonly Parser<U> delimitingParser;
        private readonly bool requiresOne;
        private readonly bool optionalTrailingDelimiter;

        public DelimitedParser(Parser<U> delimiting, Parser<T> meaningful, bool requiresOne, bool optionalTrailingDelimiter)
        {
            this.meaningfulParser = meaningful;
            this.delimitingParser = delimiting;
            this.requiresOne = requiresOne;
            this.optionalTrailingDelimiter = optionalTrailingDelimiter;
        }

        public override ResultOrParseError<IEnumerable<T>> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            List<T> output = new List<T>();
            consumed = 0;
            int skip = 0;
            while (true) {
                var result = meaningfulParser.Parse(tokens, out skip);
                if (!result.Success) {
                    if (!output.Any() && !requiresOne) {
                        return output;
                    }

                    if (optionalTrailingDelimiter) {
                        return output;
                    }

                    return new ResultOrParseError<IEnumerable<T>>(result.Error);
                }

                consumed += skip;
                tokens = tokens.Skip(skip);
                output.Add(result.Result);

                var delimit = delimitingParser.Parse(tokens, out skip);
                if (!delimit.Success) {
                    return output;
                }

                consumed += skip;
                tokens = tokens.Skip(skip);
            }
        }
    }
}
