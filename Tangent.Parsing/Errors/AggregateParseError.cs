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

        public AggregateParseError Concat(ParseError error)
        {
            var agg = error as AggregateParseError;
            if (agg != null) {
                return new AggregateParseError(Errors.Concat(agg.Errors));
            }

            return new AggregateParseError(Errors.Concat(new[] { error }));
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Errors);
        }
    }
}
