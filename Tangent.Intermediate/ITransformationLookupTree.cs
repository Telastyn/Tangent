using System.Collections.Generic;

namespace Tangent.Intermediate
{
    public interface ITransformationLookupTree
    {
        IEnumerable<IEnumerable<TransformationRule>> Lookup(IEnumerable<Expression> phrase);
    }
}