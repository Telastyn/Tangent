using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LocalAssignmentExpression : Expression
    {
        public readonly LocalAccessExpression Local;
        public readonly Expression Value;

        public LocalAssignmentExpression(LocalAccessExpression local, Expression value) : base(local.SourceInfo.Combine(value.SourceInfo))
        {
            Local = local;
            Value = value;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return TangentType.Void;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.LocalAssignment;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }
    }
}
