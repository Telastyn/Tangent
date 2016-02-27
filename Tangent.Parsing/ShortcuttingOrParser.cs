using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class ShortcuttingOrParser<T> : Parser<T>
    {
        public readonly IEnumerable<Parser<T>> Options;
        public readonly string LabelWhenNotFound;

        public ShortcuttingOrParser(IEnumerable<Parser<T>> options, string labelWhenNotFound)
        {
            if (!options.Any()) { throw new InvalidOperationException("Options must have at least one option."); }
            this.Options = new List<Parser<T>>(options);
            LabelWhenNotFound = labelWhenNotFound;
        }

        public override ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            foreach (var entry in Options) {
                var result = entry.Parse(tokens, out consumed);
                if (result.Success) {
                    return result;
                }
            }

            consumed = 0;
            return new ResultOrParseError<T>(new ExpectedLiteralParseError(LabelWhenNotFound, tokens.FirstOrDefault()));
        }
    }
}
