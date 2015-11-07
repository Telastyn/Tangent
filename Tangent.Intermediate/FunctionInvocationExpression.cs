using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class FunctionInvocationExpression : Expression
    {
        public readonly IEnumerable<Expression> Arguments;
        public readonly IEnumerable<TangentType> GenericArguments;
        public readonly ReductionDeclaration FunctionDefinition;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.FunctionInvocation; }
        }

        private TangentType returnType = null;
        public override TangentType EffectiveType
        {
            get
            {
                returnType = returnType ?? ResolveReturnType();
                return returnType;
            }
        }

        public FunctionInvocationExpression(ReductionDeclaration function, IEnumerable<Expression> arguments, IEnumerable<TangentType> genericArguments, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            FunctionDefinition = function;
            GenericArguments = genericArguments;
            Arguments = arguments;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { return; }
            workset.Add(this);

            Function replacement = null;
            if (replacements.TryGetValue(FunctionDefinition.Returns, out replacement)) {
                FunctionDefinition.Returns = replacement;
            }

            FunctionDefinition.Returns.ReplaceTypeResolvedFunctions(replacements, workset);

            foreach (var entry in Arguments) {
                entry.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }

        public override string ToString()
        {
            return string.Format("invoke {1} with ({0})", string.Join(", ", Arguments), FunctionDefinition);
        }

        private TangentType ResolveReturnType()
        {
            if (this.GenericArguments.Any()) {
                var mapping = FunctionDefinition.GenericParameters.Zip(GenericArguments, (a, b) => new KeyValuePair<ParameterDeclaration, TangentType>(a, b)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return this.FunctionDefinition.Returns.EffectiveType.ResolveGenericReferences(pd => mapping[pd]);
            } else {
                return this.FunctionDefinition.Returns.EffectiveType;
            }
        }
    }
}
