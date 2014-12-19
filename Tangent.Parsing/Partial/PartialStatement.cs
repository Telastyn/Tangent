using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialStatement
    {
        public readonly IEnumerable<Identifier> FlatTokens;
        public PartialStatement(IEnumerable<Identifier> flatTokens)
        {
            FlatTokens = flatTokens;
        }
    }
}
