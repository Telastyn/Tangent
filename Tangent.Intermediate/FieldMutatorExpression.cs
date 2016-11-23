using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldMutatorExpression : Expression
    {
        public readonly TangentType OwningType;
        public readonly Field TargetField;

        public FieldMutatorExpression(ProductType type, Field targetField) : base(null)
        {
            OwningType = type;
            TargetField = targetField;
        }

        public FieldMutatorExpression(BoundGenericType type, Field targetField) : base(null)
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
