using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial {
    public class PartialReductionDeclaration : ReductionRule<PartialPhrasePart, PartialFunction> {
        public PartialReductionDeclaration(Identifier takes, PartialFunction returns) : this(new[] { new PartialPhrasePart(takes) }, returns) { }
        public PartialReductionDeclaration(PartialPhrasePart takes, PartialFunction returns) : this(new[] { takes }, returns) { }
        public PartialReductionDeclaration(IEnumerable<PartialPhrasePart> takes, PartialFunction returns) : base(takes, returns) { }
    }
}
