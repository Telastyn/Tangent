using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    internal class PartialProductType : PlaceholderType
    {
        public readonly List<PartialPhrasePart> DataConstructorParts;
        internal PartialProductType(IEnumerable<PartialPhrasePart> dataConstructorParts)
            : base()
        {
            this.DataConstructorParts = new List<PartialPhrasePart>(dataConstructorParts);
        }
    }
}
