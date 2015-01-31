using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class DictionaryFunctionLookup : IFunctionLookup
    {
        private readonly Dictionary<ReductionDeclaration, MethodInfo> lookup;

        public DictionaryFunctionLookup(Dictionary<ReductionDeclaration, MethodInfo> lookup)
        {
            this.lookup = lookup;
        }

        public MethodInfo this[ReductionDeclaration fn]
        {
            get
            {
                MethodInfo result = null;
                if (!lookup.TryGetValue(fn, out result)) {
                    return null;
                }

                return result;
            }
        }
    }
}
