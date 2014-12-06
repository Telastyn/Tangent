using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial {
    public class PartialFunction {
        public readonly IEnumerable<Identifier> EffectiveType;
        public readonly PartialBlock Implementation;

        public PartialFunction(IEnumerable<Identifier> type, PartialBlock implementation) {
            EffectiveType = type;
            Implementation = implementation;
        }
    }
}
