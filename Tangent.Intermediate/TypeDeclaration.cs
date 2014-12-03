using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class TypeDeclaration : ReductionRule<Identifier, TangentType> {
        public TypeDeclaration(Identifier takes, ReductionRule<Identifier, TangentType> reduction) : base(takes, reduction) { }
        public TypeDeclaration(Identifier takes, TangentType returns) : base(takes, returns) { }
        public TypeDeclaration(IEnumerable<Identifier> takes, TangentType returns) : base(takes, returns) { }
    }
}
