using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class IdentifierElement: PartialElement
    {
        public readonly Identifier Identifier;
        public IdentifierElement(Identifier identifier)
            : base(ElementType.Identifier)
        {
            Identifier = identifier;
        }
    }
}
