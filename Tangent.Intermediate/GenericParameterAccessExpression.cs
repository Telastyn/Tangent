using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class GenericParameterAccessExpression : Expression
    {
        public readonly ParameterDeclaration Parameter;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.GenericParameterAccess; }
        }

        public override TangentType EffectiveType
        {
            get { return GenericArgumentReferenceType.For(Parameter); }
        }

        public GenericParameterAccessExpression(ParameterDeclaration decl, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Parameter = decl;
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

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            yield break;
        }
    }
}
