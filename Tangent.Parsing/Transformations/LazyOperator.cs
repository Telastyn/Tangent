using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Transformations
{
    public class LazyOperator : TransformationRule
    {
        public override TransformationResult TryReduce(List<Expression> buffer)
        {
            // ~> (type) => type.Lazy

            if (buffer.Count > 1) {
                if (buffer[0].NodeType == ExpressionNodeType.Identifier && ((IdentifierExpression)buffer[0]).Identifier.Value == "~>") {
                    if (buffer[1].NodeType == ExpressionNodeType.TypeAccess) {
                        var arg = (TypeAccessExpression)buffer[1];
                        return new TransformationResult(2, new TypeAccessExpression(arg.TypeConstant.Value.Lazy.TypeConstant, null));
                    }
                }
            }

            return null;
        }

        public static readonly LazyOperator Common = new LazyOperator();
    }
}
