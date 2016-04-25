using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public static class EffectiveTypeExtension
    {


        public static TangentType GetEffectiveTypeIfPossible(this Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    return invoke.EffectiveType;

                case ExpressionNodeType.ParameterAccess:
                    var param = (ParameterAccessExpression)expr;
                    return param.Parameter.Returns;

                case ExpressionNodeType.FunctionBinding:
                    var binding = expr as FunctionBindingExpression;
                    return binding.EffectiveType;

                case ExpressionNodeType.HalfBoundExpression:
                    var halfBinding = expr as HalfBoundExpression;
                    if (!halfBinding.IsDone)
                    {
                        return null;
                    }

                    return halfBinding.EffectiveType;

                case ExpressionNodeType.DelegateInvocation:
                    var delegateInvoke = expr as DelegateInvocationExpression;
                    return delegateInvoke.EffectiveType;

                case ExpressionNodeType.Constant:
                    var constant = expr as ConstantExpression;
                    return constant.EffectiveType;

                case ExpressionNodeType.EnumValueAccess:
                    var valueAccess = expr as EnumValueAccessExpression;
                    return valueAccess.EnumValue;

                case ExpressionNodeType.TypeAccess:
                case ExpressionNodeType.Identifier:
                case ExpressionNodeType.Unknown:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
