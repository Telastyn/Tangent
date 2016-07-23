using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldAccessorExpression : Expression
    {
        public readonly ProductType OwningType;
        public readonly Field TargetField;

        public FieldAccessorExpression(ProductType type, Field targetField) : base(null)
        {
            OwningType = type;
            TargetField = targetField;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return TargetField.Declaration.Returns;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.FieldAccessor;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }
    }
}
