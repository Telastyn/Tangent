using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class CompositeTransformationLookupTree : ITransformationLookupTree
    {
        private readonly IEnumerable<ITransformationLookupTree> trees;

        public CompositeTransformationLookupTree(params ITransformationLookupTree[] trees)
        {
            this.trees = trees;
        }

        public IEnumerable<IEnumerable<TransformationRule>> Lookup(IEnumerable<Expression> phrase)
        {
            foreach (var tree in trees) {
                foreach (var entry in tree.Lookup(phrase)) {
                    yield return entry;
                }
            }
        }
    }
}
