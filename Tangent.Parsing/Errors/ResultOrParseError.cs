using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Errors
{
    public class ResultOrParseError<R> : ResultOrError<R, ParseError>
    {

        public ResultOrParseError(R result) : base(result) { }
        public ResultOrParseError(ParseError error) : base(error) { }

        public static implicit operator ResultOrParseError<R>(R result)
        {
            return new ResultOrParseError<R>(result);
        }
    }
}
