using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LambdaExpression : Expression
    {
        public readonly IEnumerable<ParameterDeclaration> ResolvedParameters;
        public readonly TangentType ResolvedReturnType;
        private readonly DelegateType resolvedType;
        public readonly Block Implementation;

        public LambdaExpression(IEnumerable<ParameterDeclaration> parameters, TangentType returnType, Block implementation, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            resolvedType = DelegateType.For(parameters.Select(pd => pd.Returns), returnType);
            ResolvedParameters = parameters;
            ResolvedReturnType = returnType;
            Implementation = implementation;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Lambda; }
        }

        public override TangentType EffectiveType
        {
            get { return resolvedType; }
        }

        public override string ToString()
        {
            return string.Format("{0} {{...}}", resolvedType);
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newb = Implementation.ReplaceParameterAccesses(mapping);
            if (newb == Implementation) {
                return this;
            }

            return new LambdaExpression(ResolvedParameters, ResolvedReturnType, newb, SourceInfo);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Implementation.Statements.Any(stmt => stmt.AccessesAnyParameters(parameters, workset));
        }

        public bool RequiresClosureImplementation()
        {
            var scopeParameters = new HashSet<ParameterDeclaration>(ResolvedParameters.Concat(Implementation.Locals));
            return Implementation.Statements.Any(stmt => stmt.RequiresClosureAround(scopeParameters, new HashSet<Expression>()));
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return RequiresClosureAround(parameters, workset);
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return;
            }

            workset.Add(this);

            foreach (var statement in Implementation.Statements) {
                statement.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            // Lambdas have their own locals and cannot declare ones externally.
            yield break;
        }
    }
}
