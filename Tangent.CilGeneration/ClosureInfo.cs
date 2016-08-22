using System;
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
        public int ImplementationCounter = 0;
        public readonly Action<ILGenerator> ClosureAccessor;

        public ClosureInfo(TypeBuilder closureType, Dictionary<ParameterDeclaration, PropertyCodes> closureCodes, Action<ILGenerator> closureAccessor, ClosureInfo parent = null)
        {
            ClosureType = closureType;
            ClosureCodes = closureCodes;
            Parent = parent;
            ClosureAccessor = closureAccessor;
        }
    }
}
