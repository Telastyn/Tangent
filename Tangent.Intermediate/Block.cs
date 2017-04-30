using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class Block
    {
        public readonly IEnumerable<Expression> Statements;
        private readonly IEnumerable<ParameterDeclaration> locals;

        public IEnumerable<ParameterDeclaration> Locals
        {
            get
            {
                HashSet<Expression> workset = new HashSet<Expression>();
                return locals.Concat(Statements.SelectMany(expr => expr.CollectLocals(workset)));
            }
        }

        public Block(IEnumerable<Expression> statements, IEnumerable<ParameterDeclaration> locals)
        {
            Statements = statements;
            this.locals = locals;
        }

        public Block ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = Statements.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (Statements.SequenceEqual(newbs)) {
                return this;
            }

            return new Block(newbs, locals);
        }
    }
}
