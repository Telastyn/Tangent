using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate
{
    public class SingleValueAccessor : TransformationRule
    {
        public TransformationResult TryReduce(List<Expression> buffer, TransformationScope scope)
        {
            // (enum).<identifier that is a legal enum value> => SingleValueType

            if (buffer.Count > 2) {
                if (buffer[1].NodeType == ExpressionNodeType.Identifier && ((IdentifierExpression)buffer[1]).Identifier.Value == ".") {
                    if (buffer[2].NodeType == ExpressionNodeType.Identifier) {
                        var arg0 = buffer[0].EffectiveType as TypeConstant;
                        if (arg0 != null) {
                            var enum0 = arg0.Value as EnumType;
                            if (enum0 != null) {
                                var value = ((IdentifierExpression)buffer[2]).Identifier.Value;
                                foreach (var entry in enum0.Values) {
                                    if (entry.Value == value) {
                                        return new TransformationResult(3, Enumerable.Empty<ConversionPath>(), new TypeAccessExpression(enum0.SingleValueTypeFor(entry).TypeConstant, null));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return TransformationResult.Failure;
        }

        public static readonly SingleValueAccessor Common = new SingleValueAccessor();


        public TransformationType Type
        {
            get { return TransformationType.BuiltIn; }
        }

        public int MaxTakeCount
        {
            get { return 3; }
        }
    }
}
