using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TypeInferenceExpression:Expression
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

        public TypeInferenceExpression(IEnumerable<Identifier> name, IEnumerable<Expression> typeExpr, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            InferenceName = new List<Identifier>(name);
            InferenceExpression = new List<Expression>(typeExpr);
        }
    }
}
