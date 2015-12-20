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
    }
}
