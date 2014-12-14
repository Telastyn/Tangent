using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class FunctionBindingExpression : Expression {
        public readonly IEnumerable<Expression> Parameters;
        public readonly ReductionDeclaration FunctionDefinition;
        private ReductionDeclaration reductionDeclaration;
        private IEnumerable<bool> enumerable;

        public override ExpressionNodeType NodeType {
            get { return ExpressionNodeType.FunctionBinding; }
        }

        public FunctionBindingExpression(ReductionDeclaration function, IEnumerable<Expression> parameters) {
            Parameters = parameters;
            FunctionDefinition = function;
        }
    }
}
