using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public interface TransformationRule
    {
        TransformationResult TryReduce(List<Expression> input, TransformationScope scope);
        TransformationType Type { get; }
        int MaxTakeCount { get; }
    }
}
