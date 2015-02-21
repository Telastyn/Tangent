using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class BlockElement : PartialElement
    {
        public readonly PartialBlock Block;
        public BlockElement(PartialBlock block)
            : base(ElementType.Block)
        {
            Block = block;
        }
    }
}
