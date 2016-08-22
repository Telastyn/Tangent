using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class LocalAccessExpression : Expression
    {
        public readonly ParameterDeclaration Local;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.LocalAccess; }
        }

        public override TangentType EffectiveType
        {
            get { return Local.Returns; }
        }

        public LocalAccessExpression(ParameterDeclaration local, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            Local = local;
        }

        public override string ToString()
        {
            return string.Format("param '{0}'", string.Join(" ", Local.Takes));
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
            return parameters.Contains(Local);
        }
    }
}
