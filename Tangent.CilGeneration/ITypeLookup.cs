using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public interface ITypeLookup
    {
        Type this[TangentType t] { get; }
        void BakeTypes();
        void AddGenericFunctionParameterMapping(ParameterDeclaration generic, GenericTypeParameterBuilder dotnetType);
    }
}
