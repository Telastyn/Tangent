using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {

    public class ParameterDeclaration : ReductionRule<Identifier, TangentType> {
        public ParameterDeclaration(Identifier takes, ReductionRule<Identifier, TangentType> reduction) : base(takes, reduction) { }
        public ParameterDeclaration(Identifier takes, TangentType returns) : base(takes, returns) { }
        public ParameterDeclaration(IEnumerable<Identifier> takes, TangentType returns) : base(takes, returns) { }
    }
}
