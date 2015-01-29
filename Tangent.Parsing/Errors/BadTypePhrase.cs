using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public class BadTypePhrase
    {
        public readonly IEnumerable<Identifier> TypePhrase;

        public BadTypePhrase(IEnumerable<Identifier> identifiers)
        {
            TypePhrase = identifiers;
        }
    }
}
