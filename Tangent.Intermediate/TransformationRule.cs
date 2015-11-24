using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public interface TransformationRule
    {
        TransformationResult TryReduce(List<Expression> input);
        TransformationType Type { get; }
        int MaxTakeCount { get; }
    }

    public static class ExtendTransformationRule
    {
        public static IEnumerable<TransformationRule> Sort(this IEnumerable<TransformationRule> input)
        {
            return input.OrderBy(r => (int)r.Type).OrderByDescending(r => r.MaxTakeCount);
        }
    }
}
