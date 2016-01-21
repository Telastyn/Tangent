using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class Delazy : TransformationRule
    {
        public static readonly TransformationRule Common = new Delazy();

        public TransformationResult TryReduce(List<Expression> input, TransformationScope scope)
        {
            if (!input.Any()) {
                return TransformationResult.Failure;
            }

            var firstType = input.First().EffectiveType;
            if (firstType == null || firstType.ImplementationType != KindOfType.Delegate) {
                return TransformationResult.Failure;
            }

            var firstDelegateType = firstType as DelegateType;
            if (firstDelegateType.Takes.Any()) {
                return TransformationResult.Failure;
            }

            return new TransformationResult(1, new DelegateInvocationExpression(input.First(), Enumerable.Empty<Expression>(), input.First().SourceInfo));
        }

        public TransformationType Type
        {
            get { return TransformationType.Coersion; }
        }

        public int MaxTakeCount
        {
            get { return 1; }
        }
    }
}
