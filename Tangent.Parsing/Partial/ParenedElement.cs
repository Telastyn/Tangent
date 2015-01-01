using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class ParenedElement : PartialElement
    {
        public readonly IEnumerable<PartialElement> EnclosedElements;
        public ParenedElement(IEnumerable<PartialElement> enclosed)
            : base(ElementType.Parens)
        {
            EnclosedElements = enclosed;
        }
    }
}
