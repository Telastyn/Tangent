using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class NotFollowedByParser<T, U> : Parser<T>
    {
        private readonly Parser<T> meaningfulParser;
        private readonly Parser<U> peeker;
        private readonly string onAvoidFound;

        public NotFollowedByParser(Parser<T> meaningful, Parser<U> toAvoid, string onAvoidFound)
        {
            this.meaningfulParser = meaningful;
            this.peeker = toAvoid;
            this.onAvoidFound = onAvoidFound;
        }

        public override ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            int takes;
            var result = meaningfulParser.Parse(tokens, out takes);
            if (!result.Success) {
                consumed = takes;
                return result;
            }

            int discard;
            var avoidResult = peeker.Parse(tokens.Skip(takes), out discard);
            if (avoidResult.Success) {
                consumed = 0;
                return new ResultOrParseError<T>(new ExpectedLiteralParseError(onAvoidFound, tokens.Skip(takes).FirstOrDefault()));
            }

            consumed = takes;
            return result;
        }
    }
}
