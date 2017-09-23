using System.Collections.Generic;

namespace Tangent.Intermediate
{
    public interface ITransformationLookupTree
    {
        int ApproximateRulesetSize { get; }
        IEnumerable<IEnumerable<TransformationRule>> Lookup(IEnumerable<Expression> phrase);
    }
}