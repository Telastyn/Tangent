using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class DifferenceParser<T,D>: Parser<T>
    {
        private readonly Parser<D> exceptions;
        private readonly Parser<T> production;

        public DifferenceParser(Parser<T> production, Parser<D> difference)
        {
            this.exceptions = difference;
            this.production = production;
        }

        public override ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            int discard;
            var differenceBits = exceptions.Parse(tokens, out discard);
            if (differenceBits.Success) {
                consumed = 0;
                return new ResultOrParseError<T>(new ExpectedLiteralParseError("Valid token", tokens.FirstOrDefault()));
            }

            return production.Parse(tokens, out consumed);
        }
    }
}
