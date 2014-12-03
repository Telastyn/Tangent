using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing {
    public class PartialParameterDeclaration : ReductionRule<Identifier, List<Identifier>> {
        public PartialParameterDeclaration(Identifier takes, ReductionRule<Identifier, List<Identifier>> reduction) : base(takes, reduction) { }
        public PartialParameterDeclaration(Identifier takes, List<Identifier> returns) : base(takes, returns) { }
        public PartialParameterDeclaration(IEnumerable<Identifier> takes, List<Identifier> returns) : base(takes, returns) { }
    }
}
