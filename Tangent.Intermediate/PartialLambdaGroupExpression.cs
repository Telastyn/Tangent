using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PartialLambdaGroupExpression : Expression, FitableLambda
    {
        public readonly IEnumerable<PartialLambdaExpression> Lambdas;

        public PartialLambdaGroupExpression(IEnumerable<PartialLambdaExpression> lambdas) : base(LineColumnRange.CombineAll(lambdas.Select(x => x.SourceInfo)))
        {
            if (lambdas == null || !lambdas.Any()) {
                throw new InvalidOperationException("Lambda groups must have at least one lambda.");
            }
            Lambdas = lambdas;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Lambdas.Any(l => l.AccessesAnyParameters(parameters, workset));
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            return Lambdas.Aggregate(Enumerable.Empty<ParameterDeclaration>(), (a, l) => a.Concat(l.CollectLocals(workset)));
        }

        public override TangentType EffectiveType
        {
            get
            {
                return TangentType.PotentiallyAnything;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.PartialLambdaGroup;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this; // works? no idea.
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override string ToString()
        {
            return $":< {string.Join(" ", Lambdas.First().Parameters.First().Takes.Select(pp => pp.Identifier))} {{ {string.Join("; ", Lambdas) } }}";
        }

        public Expression TryToFitIn(TangentType target)
        {
            var targetDelegate = target as DelegateType;
            if (targetDelegate == null) {
                return null;
            }

            // For each lambda, we need to infer generics (generally, the return type, but also the parameter type for default lambdas) and make sure that the body actually works.
            List<LambdaExpression> resolved = new List<LambdaExpression>();
            foreach (var entry in Lambdas) {
                var entryFit = entry.TryToFitIn(target, true);
                if (entryFit == null) {
                    return entryFit;
                }

                if (entryFit is AmbiguousExpression) {
                    return entryFit;
                }

                if (entryFit is LambdaExpression) {
                    resolved.Add(entryFit as LambdaExpression);
                } else {
                    throw new ApplicationException("Unknown result from lambda fit.");
                }
            }

            return new LambdaGroupExpression(targetDelegate, resolved);

        }
    }
}
