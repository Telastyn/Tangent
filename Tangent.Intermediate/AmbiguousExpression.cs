using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class AmbiguousExpression : Expression
    {
        public readonly IEnumerable<Expression> PossibleInterpretations;

        public AmbiguousExpression(IEnumerable<Expression> exprs)
            : base(exprs.First().SourceInfo)
        {
            PossibleInterpretations = exprs;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Ambiguity; }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return null;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = PossibleInterpretations.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (newbs.SequenceEqual(PossibleInterpretations)) {
                return this;
            }

            return new AmbiguousExpression(newbs);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            yield break;
        }
    }
}
