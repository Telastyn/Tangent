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

        public AmbiguousStatementError(IEnumerable<Expression> statement, IEnumerable<Expression> interpretations)
            : base(statement)
        {
            PossibleInterpretations = interpretations;
        }

        public override string ToString()
        {
            return string.Format("Ambiguous statement: {0}{1}{2}", string.Join(" ", base.ErrorLocation), Environment.NewLine, string.Join(Environment.NewLine, PossibleInterpretations.Select(expr=> "  " + expr.ToString())));
        }
    }
}
