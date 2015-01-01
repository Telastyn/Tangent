using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialStatement
    {
        public readonly IEnumerable<PartialElement> FlatTokens;
        public PartialStatement(IEnumerable<PartialElement> flatTokens)
        {
            FlatTokens = flatTokens;
        }
    }
}
