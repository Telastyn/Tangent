using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class BoolEnumAdapterType : EnumType
    {
        internal static BoolEnumAdapterType Common = null;
        internal BoolEnumAdapterType() : base(new Identifier[] { "false", "true" }) { }
        protected override int NumericEquivalenceOf(Identifier id)
        {
            if (id.Value == "false") { return 0; } else { return 1; }
        }
    }
}
