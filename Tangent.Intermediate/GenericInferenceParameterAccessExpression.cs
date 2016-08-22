using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericInferenceParameterAccessExpression : Expression
    {
        public readonly GenericInferencePlaceholder Inference;

        public override TangentType EffectiveType
        {
            get { return Inference; }
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.TypeInference; }
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // nada.
        }

        public GenericInferenceParameterAccessExpression(GenericInferencePlaceholder inference, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Inference = inference;
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
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
