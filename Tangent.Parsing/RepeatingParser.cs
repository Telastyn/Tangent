using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class RepeatingParser<T>: Parser<IEnumerable<T>>
    {
        public readonly bool RequireOne;
        private readonly Parser<T> InstanceParser;

        public RepeatingParser(Parser<T> instanceParser, bool requireOne)
        {
            this.RequireOne = requireOne;
            this.InstanceParser = instanceParser;
        }

        public override ResultOrParseError<IEnumerable<T>> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            List<T> output = new List<T>();
            bool go = false;
            consumed = 0;
            ResultOrParseError<T> result = null;
            do {
                go = false;
                int taken = 0;
                result = InstanceParser.Parse(tokens, out taken);
                if (result.Success) {
                    go = true;
                    output.Add(result.Result);
                    tokens = tokens.Skip(taken);
                    consumed += taken;
                }
            } while (go);

            if (RequireOne && !output.Any()) {
                return new ResultOrParseError<IEnumerable<T>>(result.Error);
            }

            return new ResultOrParseError<IEnumerable<T>>(output);
        }
    }
}
