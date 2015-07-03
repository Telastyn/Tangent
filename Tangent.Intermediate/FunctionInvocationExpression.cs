using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class FunctionInvocationExpression : Expression
    {
        public readonly FunctionBindingExpression Bindings;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.FunctionInvocation; }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return Bindings.ReturnType;
            }
        }

        public FunctionInvocationExpression(FunctionBindingExpression binding)
            : base(binding.SourceInfo)
        {
            Bindings = binding;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { return; }
            workset.Add(this);
            Bindings.ReplaceTypeResolvedFunctions(replacements, workset);
        }
    }
}
