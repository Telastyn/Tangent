using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing {
    public class PartialFunctionParse {
        public readonly IEnumerable<Identifier> TypeExpression;
        public readonly IEnumerable<IEnumerable<Identifier>> Block;

        public PartialFunctionParse(IEnumerable<Identifier> typeExpr, IEnumerable<IEnumerable<Identifier>> block) {
            TypeExpression = typeExpr;
            Block = block;
        }
    }
}
