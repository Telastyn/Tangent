using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial {
    public class PartialReductionDeclaration : ReductionRule<PhrasePart, PartialFunction> {
        public PartialReductionDeclaration(PhrasePart takes, ReductionRule<PhrasePart, PartialFunction> reduction) : base(takes, reduction) { }
        public PartialReductionDeclaration(PhrasePart takes, PartialFunction returns) : base(takes, returns) { }
        public PartialReductionDeclaration(IEnumerable<PhrasePart> takes, PartialFunction returns) : base(takes, returns) { }
    }
}
