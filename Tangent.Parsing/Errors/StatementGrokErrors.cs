using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Errors
{
    public class StatementGrokErrors : ParseError
    {
        public readonly IEnumerable<IncomprehensibleStatementError> IncomprehensibleStatements;
        public readonly IEnumerable<AmbiguousStatementError> AmbiguousStatements;

        public StatementGrokErrors(IEnumerable<IncomprehensibleStatementError> incomprehensible, IEnumerable<AmbiguousStatementError> ambiguous)
        {
            IncomprehensibleStatements = incomprehensible;
            AmbiguousStatements = ambiguous;
        }
    }
}
