using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PartialLambdaGroupExpression : Expression
    {
        public readonly Expression InputExpr;
        public readonly IEnumerable<PartialLambdaExpression> Lambdas;

        public PartialLambdaGroupExpression(Expression input, IEnumerable<PartialLambdaExpression> lambdas):base(LineColumnRange.Combine(input.SourceInfo, lambdas.Select(x => x.SourceInfo)))
        {
            InputExpr = input;
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
            return $":< {InputExpr} {{ {string.Join("; ", Lambdas) } }}";
        }
    }
}
