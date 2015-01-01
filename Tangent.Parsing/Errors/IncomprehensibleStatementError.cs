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
    }
}
