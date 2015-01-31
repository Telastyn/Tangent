using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public interface IFunctionLookup
    {
        MethodInfo this[ReductionDeclaration fn] { get; }
    }
}
