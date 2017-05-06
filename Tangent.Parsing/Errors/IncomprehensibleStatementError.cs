using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public class IncomprehensibleStatementError : StatementParseError
    {
        public IncomprehensibleStatementError(IEnumerable<Expression> statement)
            : base(statement)
        {

        }

        public override string ToString()
        {
            return string.Format("Unable to interpret statement: {0} at {1}", string.Join(" ", base.ErrorLocation), LineColumnRange.CombineAll(base.ErrorLocation.Select(expr=>expr.SourceInfo)));
        }
    }
}
