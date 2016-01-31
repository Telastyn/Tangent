using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialProductType : PartialClass
    {
        public readonly List<PartialPhrasePart> DataConstructorParts;

        internal PartialProductType(IEnumerable<PartialPhrasePart> dataConstructorParts, IEnumerable<PartialReductionDeclaration> functions, IEnumerable<PartialParameterDeclaration> genericArgs)
            : base(functions, genericArgs)
        {
            this.DataConstructorParts = new List<PartialPhrasePart>(dataConstructorParts);
        }
    }
}
