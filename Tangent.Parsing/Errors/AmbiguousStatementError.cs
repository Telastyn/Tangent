using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public class AmbiguousStatementError : StatementParseError
    {
        public readonly IEnumerable<Expression> PossibleInterpretations;

        public AmbiguousStatementError(IEnumerable<Identifier> statement, IEnumerable<Expression> interpretations)
            : base(statement)
        {
            PossibleInterpretations = interpretations;
        }
    }
}
