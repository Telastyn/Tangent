using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class Block {
        public readonly IEnumerable<Statement> Statements;

        public Block(IEnumerable<Statement> statements) {
            Statements = statements;
        }
    }
}
