using System.Collections.Generic;

namespace Tangent.Intermediate
{
    public interface TransformationScope
    {
        int ApproximateRulesetSize { get; }
        ConversionGraph Conversions { get; }
        TransformationScope CreateNestedLocalScope(IEnumerable<ParameterDeclaration> locals);
        TransformationScope CreateNestedParameterScope(IEnumerable<ParameterDeclaration> parameters);
        List<Expression> InterpretTowards(TangentType target, List<Expression> input);
    }

    public static class ExtendTransformationScopeStrategy
    {
        public static List<Expression> InterpretStatement(this TransformationScope scope, List<Expression> input)
        {
            return scope.InterpretTowards(TangentType.Void, input);
        }
    }
}