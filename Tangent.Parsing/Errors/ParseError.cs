using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public class ParseError
    {
    }

    public abstract class StatementParseError : ParseError
    {
        public readonly IEnumerable<Expression> ErrorLocation;

        protected StatementParseError(IEnumerable<Expression> statement)
        {
            ErrorLocation = statement;
        }
    }
}
