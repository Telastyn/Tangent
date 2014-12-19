using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class FunctionInvocationExpression : Expression {
        public readonly FunctionBindingExpression Bindings;

        public override ExpressionNodeType NodeType {
            get { return ExpressionNodeType.FunctionInvocation; }
        }

        public TangentType EffectiveType {
            get {
                return Bindings.FunctionDefinition.Returns.EffectiveType;
            }
        }

        public FunctionInvocationExpression(FunctionBindingExpression binding) {
            Bindings = binding;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset) {
            if (workset.Contains(this)) { return; }
            workset.Add(this);
            Bindings.ReplaceTypeResolvedFunctions(replacements, workset);
        }
    }
}
