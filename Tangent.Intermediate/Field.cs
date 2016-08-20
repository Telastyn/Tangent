using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class Field
    {
        public readonly ParameterDeclaration Declaration;
        public Expression Initializer { get; private set; }

        public Field(ParameterDeclaration decl, Expression initializer)
        {
            Declaration = decl;
            Initializer = initializer;
        }

        public void ResolveInitializerPlaceholders(Func<Expression, Expression> resolver)
        {
            // gross.
            if (Initializer.NodeType == ExpressionNodeType.InitializerPlaceholder) {
                Initializer = resolver(Initializer);
            }
        }

        internal virtual void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(Initializer)) {
                return;
            }

            workset.Add(Initializer);

            Initializer.ReplaceTypeResolvedFunctions(replacements, workset);
        }
    }
}
