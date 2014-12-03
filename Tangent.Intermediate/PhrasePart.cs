using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class PhrasePart {
        public readonly Identifier Identifier;
        public readonly ParameterDeclaration Parameter;
        public bool IsIdentifier { get { return Identifier != null; } }

        public PhrasePart(Identifier id) {
            Identifier = id;
        }

        public PhrasePart(ParameterDeclaration decl) {
            Parameter = decl;
        }
    }
}
