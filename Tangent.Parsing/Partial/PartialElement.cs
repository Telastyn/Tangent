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
        VarDecl,
        Block,
        Constant,
        Lambda,
        LambdaGroup
    }

    public abstract class PartialElement
    {
        public readonly ElementType Type;
        public readonly LineColumnRange SourceInfo;
        public PartialElement(ElementType type, LineColumnRange sourceInfo)
        {
            Type = type;
            SourceInfo = sourceInfo;
        }
    }
}
