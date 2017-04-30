using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldAccessorExpression : Expression
    {
        public readonly TangentType OwningType;
        public readonly Field TargetField;

        public FieldAccessorExpression(ProductType type, Field targetField) : base(null)
        {
            OwningType = type;
            TargetField = targetField;
        }

        public FieldAccessorExpression(BoundGenericType type, Field targetField) : base(null)
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
