using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class FunctionBindingExpression : Expression
    {
        public readonly IEnumerable<Expression> Parameters;
        public readonly ReductionDeclaration FunctionDefinition;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.FunctionBinding; }
        }

        public FunctionBindingExpression(ReductionDeclaration function, IEnumerable<Expression> parameters)
        {
            Parameters = parameters;
            FunctionDefinition = function;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { return; }
            workset.Add(this);

            Function replacement = null;
            if (replacements.TryGetValue(FunctionDefinition.Returns, out replacement)) {
                FunctionDefinition.Returns = replacement;
            }

            foreach (var entry in Parameters) {
                entry.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }
    }
}
