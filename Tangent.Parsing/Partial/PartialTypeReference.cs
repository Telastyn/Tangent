using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialTypeReference : PlaceholderType
    {
        public IEnumerable<Identifier> Identifiers;
        public TangentType ResolvedType = null;

        public PartialTypeReference(IEnumerable<Identifier> parts)
        {
            Identifiers = new List<Identifier>(parts);
        }
    }
}
