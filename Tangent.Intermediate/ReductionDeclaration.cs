using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionDeclaration : ReductionRule<Identifier, Function> {
        public ReductionDeclaration(Identifier takes, ReductionRule<Identifier, Function> reduction) : base(takes, reduction) { }
        public ReductionDeclaration(Identifier takes, Function returns) : base(takes, returns) { }
        public ReductionDeclaration(IEnumerable<Identifier> takes, Function returns) : base(takes, returns) { }
    }
}
