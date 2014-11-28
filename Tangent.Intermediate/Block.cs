using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class Block {
        public readonly IEnumerable<PartialStatement> Statements;

        public Block(IEnumerable<PartialStatement> statements) {
            Statements = statements;
        }
    }
}
