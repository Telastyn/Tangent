using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Errors
{
    public class AggregateParseError : ParseError
    {
        public readonly IEnumerable<ParseError> Errors;
        public AggregateParseError(IEnumerable<ParseError> errors)
        {
            Errors = errors;
        }
    }
}
