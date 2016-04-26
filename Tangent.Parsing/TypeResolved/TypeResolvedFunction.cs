using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.TypeResolved
{
    public class TypeResolvedFunction : Function
    {
        public new readonly TangentType EffectiveType;
        public new readonly PartialBlock Implementation;
        public readonly TangentType Scope;

        public TypeResolvedFunction(TangentType type, PartialBlock implementation, TangentType scope)
            : base(type, null)
        {
            EffectiveType = type;
            Implementation = implementation;
            Scope = scope;
        }
    }
}
