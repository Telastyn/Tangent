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

        public override TangentType EffectiveType
        {
            get
            {
                return FunctionDefinition.Returns.EffectiveType.Lazy;
            }
        }

        public FunctionBindingExpression(ReductionDeclaration function, IEnumerable<Expression> parameters, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Parameters = parameters.ToList();
            FunctionDefinition = function;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { return; }
            workset.Add(this);

            Function replacement = null;
            if (replacements.TryGetValue(FunctionDefinition.Returns, out replacement))
            {
                FunctionDefinition.Returns = replacement;
            }

            FunctionDefinition.Returns.ReplaceTypeResolvedFunctions(replacements, workset);

            foreach (var entry in Parameters)
            {
                entry.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }
    }
}
