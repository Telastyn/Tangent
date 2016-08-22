using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialTypeInferenceExpression : Expression
    {
        public readonly IEnumerable<Identifier> InferenceName;
        public readonly IEnumerable<Expression> InferenceExpression;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.TypeInference; }
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // nada.
        }

        public override TangentType EffectiveType
        {
            get { return null; }
        }

        public PartialTypeInferenceExpression(IEnumerable<Identifier> name, IEnumerable<Expression> typeExpr, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            InferenceName = new List<Identifier>(name);
            InferenceExpression = new List<Expression>(typeExpr);
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newinf = InferenceExpression.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (InferenceExpression.SequenceEqual(newinf)) {
                return this;
            }

            return new PartialTypeInferenceExpression(InferenceName, newinf, SourceInfo);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }
    }
}
