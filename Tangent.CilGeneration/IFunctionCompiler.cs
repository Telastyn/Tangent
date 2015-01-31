using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public interface IFunctionCompiler
    {
        void BuildFunctionImplementation(ReductionDeclaration fn, MethodBuilder target, IEnumerable<ReductionDeclaration> specializations, TypeBuilder closureScope, IFunctionLookup fnLookup, ITypeLookup typeLookup);
    }
}
