using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class Block
    {
        public readonly IEnumerable<Expression> Statements;

        public Block(IEnumerable<Expression> statements)
        {
            Statements = statements;
        }
    }
}
