using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    internal class PlaceholderType : TangentType
    {
        protected PlaceholderType() : base(KindOfType.Placeholder) { }
    }
}
