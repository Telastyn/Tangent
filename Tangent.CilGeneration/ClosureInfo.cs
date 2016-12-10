using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class ClosureInfo
    {
        public readonly TypeBuilder ClosureType;
        public readonly Dictionary<ParameterDeclaration, PropertyCodes> ClosureCodes;
        public readonly ClosureInfo Parent;
        public int ImplementationCounter
        {
            get
            {
                return counters.GetOrAdd(ClosureType, 0);
            }
            set
            {
                counters.AddOrUpdate(ClosureType, value, (tb, v) => value);
            }
        }

        public readonly Action<ILGenerator> ClosureAccessor;

        public ClosureInfo(TypeBuilder closureType, Dictionary<ParameterDeclaration, PropertyCodes> closureCodes, Action<ILGenerator> closureAccessor, ClosureInfo parent = null)
        {
            ClosureType = closureType;
            ClosureCodes = closureCodes;
            Parent = parent;
            ClosureAccessor = closureAccessor;
        }

        private static readonly ConcurrentDictionary<TypeBuilder, int> counters = new ConcurrentDictionary<TypeBuilder, int>();
    }
}
