using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ParameterAccessExpression : Expression
    {
        public readonly ParameterDeclaration Parameter;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.ParameterAccess; }
        }

        private readonly TangentType effectiveType;

        public override TangentType EffectiveType
        {
            get
            {
                return effectiveType;
            }
        }

        public ParameterAccessExpression(ParameterDeclaration decl, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Parameter = decl;
            effectiveType = decl.Returns.RebindInferences(pd => GenericArgumentReferenceType.For(pd));
        }

        public override string ToString()
        {
            return string.Format("param '{0}'", string.Join(" ", Parameter.Takes));
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            if (!mapping.ContainsKey(Parameter)) {
                return this;
            }

            return mapping[Parameter];
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return parameters.Contains(Parameter);
        }
    }
}
