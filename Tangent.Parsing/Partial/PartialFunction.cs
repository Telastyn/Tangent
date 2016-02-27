using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialFunction
    {
        public readonly IEnumerable<Expression> EffectiveType;
        public readonly PartialClass Scope;
        public readonly PartialBlock Implementation;

        public PartialFunction(IEnumerable<Expression> type, PartialBlock implementation, PartialClass scope)
        {
            EffectiveType = type;
            Implementation = implementation;
            Scope = scope;
        }
    }
}
