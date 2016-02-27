using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class DelegatingParser<T> : Parser<T>
    {
        private Func<Parser<T>> wrapped;
        public DelegatingParser(Func<Parser<T>> closure)
        {
            this.wrapped = closure;
        }

        public override ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            return wrapped().Parse(tokens, out consumed);
        }
    }
}
