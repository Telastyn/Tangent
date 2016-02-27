using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class StringLiteralParser : Parser<string>
    {
        private readonly string target;
        public StringLiteralParser(string target)
        {
            this.target = target;
        }

        public override ResultOrParseError<string> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Value != target) {
                consumed = 0;
                return new ResultOrParseError<string>(new ExpectedLiteralParseError(target, first));
            }

            consumed = 1;
            return target;
        }
    }
}
