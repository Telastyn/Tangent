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
                return Bindings.FunctionDefinition.EndResult().EffectiveType;
            }
        }

        public FunctionInvocationExpression(FunctionBindingExpression binding) {
            Bindings = binding;
        }
    }
}
