using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class Function
    {
        public readonly TangentType EffectiveType;
        public readonly Block Implementation;

        public Function(TangentType type, Block implementation)
        {
            if(type == null) { throw new NotImplementedException(); }
            EffectiveType = type;
            Implementation = implementation;
        }

        internal virtual void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (Implementation == null) { return; }
            foreach (var entry in Implementation.Statements) {
                entry.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }
    }
}
