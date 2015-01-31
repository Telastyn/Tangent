using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class EmptyFunctionLookup : IFunctionLookup
    {
        public static readonly EmptyFunctionLookup Common = new EmptyFunctionLookup();

        public MethodInfo this[ReductionDeclaration fn]
        {
            get { return null; }
        }
    }
}
