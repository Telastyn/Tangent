using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial {
    public class PartialReductionDeclaration : ReductionRule<PartialPhrasePart, PartialFunction> {
        public PartialReductionDeclaration(PartialPhrasePart takes, ReductionRule<PartialPhrasePart, PartialFunction> reduction) : base(takes, reduction) { }
        public PartialReductionDeclaration(PartialPhrasePart takes, PartialFunction returns) : base(takes, returns) { }
        public PartialReductionDeclaration(IEnumerable<PartialPhrasePart> takes, PartialFunction returns) : base(takes, returns) { }
    }
}
