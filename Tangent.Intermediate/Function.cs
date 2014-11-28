using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class Function {
        public readonly TangentType EffectiveType;
        public readonly Block Implementation;

        public Function(TangentType type, Block implementation) {
            EffectiveType = type;
            Implementation = implementation;
        }
    }
}
