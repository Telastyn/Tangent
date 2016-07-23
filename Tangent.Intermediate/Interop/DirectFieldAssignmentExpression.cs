using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectFieldAssignmentExpression:Expression
    {
        public readonly FieldInfo Field;
        public readonly IEnumerable<Expression> Arguments;

        public DirectFieldAssignmentExpression(FieldInfo field, IEnumerable<Expression> args) : base(null)
        {
            Field = field;
            Arguments = args;
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
                return ExpressionNodeType.DirectFieldAssignment;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = Arguments.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (Arguments.SequenceEqual(newbs)) {
                return this;
            }

            return new DirectFieldAssignmentExpression(Field, newbs);
        }
    }
}
