using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Partial {
    public class PartialBlock {
        public readonly IEnumerable<PartialStatement> Statements;

        public PartialBlock(IEnumerable<PartialStatement> statements) {
            Statements = statements;
        }
    }
}
