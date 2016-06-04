using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Partial
{
    public class PartialBlock
    {
        public readonly IEnumerable<PartialStatement> Statements;
        public readonly IEnumerable<VarDeclElement> Locals;

        public PartialBlock(IEnumerable<PartialStatement> statements, IEnumerable<VarDeclElement> locals)
        {
            Statements = statements;
            Locals = locals;
        }
    }
}
