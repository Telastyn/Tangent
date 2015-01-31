using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public enum ElementType
    {
        Identifier,
        Parens,
        Block,
        Constant
    }

    public abstract class PartialElement
    {
        public readonly ElementType Type;
        public PartialElement(ElementType type)
        {
            Type = type;
        }
    }
}
