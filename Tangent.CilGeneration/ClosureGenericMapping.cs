using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class ClosureGenericMapping
    {
        public readonly ParameterDeclaration TangentGeneric;
        public readonly GenericTypeParameterBuilder ClosureGeneric;
        public readonly Type FunctionGeneric;

        public ClosureGenericMapping(ParameterDeclaration tangentGeneric, Type functionGeneric, GenericTypeParameterBuilder closureGeneric)
        {
            FunctionGeneric = functionGeneric;
            ClosureGeneric = closureGeneric;
            TangentGeneric = tangentGeneric;
        }
    }
}
