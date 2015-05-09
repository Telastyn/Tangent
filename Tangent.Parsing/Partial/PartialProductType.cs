using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialProductType : PlaceholderType
    {
        public readonly List<PartialPhrasePart> DataConstructorParts;
        public readonly List<PartialReductionDeclaration> Functions;

        internal PartialProductType(IEnumerable<PartialPhrasePart> dataConstructorParts, IEnumerable<PartialReductionDeclaration> functions)
            : base()
        {
            this.DataConstructorParts = new List<PartialPhrasePart>(dataConstructorParts);
            this.Functions = new List<PartialReductionDeclaration>(functions);
        }
    }
}
