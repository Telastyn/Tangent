using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldMutatorExpression : Expression
    {
        public readonly ProductType OwningType;
        public readonly Field TargetField;

        public FieldMutatorExpression(ProductType type, Field targetField) : base(null)
        {
            OwningType = type;
            TargetField = targetField;
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
                return ExpressionNodeType.FieldMutator;
            }
        }
    }
}
