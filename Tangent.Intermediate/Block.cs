using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class Block
    {
        public readonly IEnumerable<Expression> Statements;
        public readonly IEnumerable<ParameterDeclaration> Locals;

        public Block(IEnumerable<Expression> statements, IEnumerable<ParameterDeclaration> locals)
        {
            Statements = statements;
            Locals = locals;
        }
    }
}
