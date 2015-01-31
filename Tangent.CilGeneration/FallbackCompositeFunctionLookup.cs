using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class FallbackCompositeFunctionLookup:IFunctionLookup
    {
        private readonly IFunctionLookup primary;
        private readonly IFunctionLookup secondary;
        public FallbackCompositeFunctionLookup(IFunctionLookup primary, IFunctionLookup secondary)
        {
            this.primary = primary;
            this.secondary = secondary;
        }

        public MethodInfo this[ReductionDeclaration fn]
        {
            get { return primary[fn] ?? secondary[fn]; }
        }
    }
}
