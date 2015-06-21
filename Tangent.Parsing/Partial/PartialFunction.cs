using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialFunction
    {
        public readonly IEnumerable<IdentifierExpression> EffectiveType;
        public readonly PartialProductType Scope;
        public readonly PartialBlock Implementation;

        public PartialFunction(IEnumerable<IdentifierExpression> type, PartialBlock implementation, PartialProductType scope)
        {
            EffectiveType = type;
            Implementation = implementation;
            Scope = scope;
        }
    }
}
