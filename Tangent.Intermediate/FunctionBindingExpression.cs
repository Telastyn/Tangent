using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class FunctionBindingExpression : Expression
    {
        public readonly IEnumerable<Expression> Arguments;
        public readonly IEnumerable<TangentType> GenericArguments;
        public readonly ReductionDeclaration FunctionDefinition;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.FunctionBinding; }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return ReturnType.Lazy;
            }
        }

        private TangentType returnType = null;

        public TangentType ReturnType
        {
            get
            {
                returnType = returnType ?? ResolveReturnType();
                return returnType;
            }
        }

        public FunctionBindingExpression(ReductionDeclaration function, IEnumerable<Expression> arguments, LineColumnRange sourceInfo)
            : this(function,arguments,Enumerable.Empty<TangentType>(), sourceInfo)
        {
        }

        public FunctionBindingExpression(ReductionDeclaration function, IEnumerable<Expression> arguments, IEnumerable<TangentType> genericArguments, LineColumnRange sourceInfo):base(sourceInfo)
        {
            if (function.GenericParameters.Count() != genericArguments.Count()) { throw new InvalidOperationException("Generic parameter and argument mismatch in Function Binding."); }

            Arguments = arguments.ToList();
            GenericArguments = genericArguments.ToList();
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

            foreach (var entry in Arguments)
            {
                entry.ReplaceTypeResolvedFunctions(replacements, workset);
            }
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
