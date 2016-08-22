using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumWideningExpression : Expression
    {
        public readonly EnumValueAccessExpression EnumAccess;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.EnumWidening; }
        }

        public override TangentType EffectiveType
        {
            get { return EnumAccess.EnumValue.ValueType; }
        }

        public EnumWideningExpression(EnumValueAccessExpression expr)
            : base(expr.SourceInfo)
        {
            this.EnumAccess = expr;
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
