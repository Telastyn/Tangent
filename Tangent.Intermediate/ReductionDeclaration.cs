using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionDeclaration : ReductionRule<PhrasePart, Function> {
        public ReductionDeclaration(PhrasePart takes, ReductionRule<PhrasePart, Function> reduction) : base(takes, reduction) { }
        public ReductionDeclaration(PhrasePart takes, Function returns) : base(takes, returns) { }
        public ReductionDeclaration(IEnumerable<PhrasePart> takes, Function returns) : base(takes, returns) { }
    }
}
