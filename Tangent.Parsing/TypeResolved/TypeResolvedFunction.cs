using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.TypeResolved {
    public class TypeResolvedFunction : Function {
        public readonly TangentType EffectiveType;
        public readonly PartialBlock Implementation;

        public TypeResolvedFunction(TangentType type, PartialBlock implementation)
            : base(type, null) {
            EffectiveType = type;
            Implementation = implementation;
        }
    }
}
