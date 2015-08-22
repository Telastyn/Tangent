using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class CtorCall : Function
    {
        public CtorCall(BoundGenericProductType type): base(type, null){ }
        public CtorCall(ProductType type) : base(type, null) { }
        public CtorCall(SumType type) : base(type, null) { }
        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // nada.
        }
    }
}
