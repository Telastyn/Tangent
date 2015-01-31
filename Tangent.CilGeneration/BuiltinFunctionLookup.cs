using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class BuiltinFunctionLookup : IFunctionLookup
    {
        public static BuiltinFunctionLookup Common = new BuiltinFunctionLookup();
        public MethodInfo this[ReductionDeclaration fn]
        {
            get { return BuiltinFunctions.DotNetFunctionForBuiltin(fn); }
        }
    }
}
