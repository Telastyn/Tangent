using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LambdaGroupExpression : Expression
    {
        private readonly DelegateType effectiveType;
        public readonly IEnumerable<LambdaExpression> Lambdas;

        public LambdaGroupExpression(TangentType inferredInputType, IEnumerable<LambdaExpression> lambdas) : base(LineColumnRange.CombineAll(lambdas.Select(l => l.SourceInfo)))
        {
            if (!lambdas.Any()) {
                throw new InvalidOperationException("Lambda Groups must have at least one lambda.");
            }

            effectiveType = DelegateType.For(new[] { inferredInputType }, lambdas.First().ResolvedReturnType);
            Lambdas = lambdas;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Lambdas.Any(l => l.AccessesAnyParameters(parameters, workset));
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            // Lambdas have their own locals and cannot declare ones externally.
            yield break;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return effectiveType;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.LambdaGroup;
            }
        }

        public override string ToString()
        {
            return $"{{ {string.Join("; ", Lambdas) } }}";
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            bool newb = false;
            var newLambdas = new List<LambdaExpression>();
            foreach (var lambda in Lambdas) {
                var replaced = lambda.ReplaceParameterAccesses(mapping);
                newLambdas.Add((LambdaExpression)replaced);
                if (replaced != lambda) {
                    newb = true;
                }
            }

            if (!newb) {
                return this;
            }

            return new LambdaGroupExpression(effectiveType.Takes.First(), newLambdas);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Lambdas.Any(l => l.RequiresClosureAround(parameters, workset));
        }
    }
}
