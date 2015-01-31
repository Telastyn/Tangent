using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate
{
    public static class BuiltinFunctions
    {
        public static ReductionDeclaration PrintString = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("s", TangentType.String) }, new Function(TangentType.Void, null));

        private static readonly Dictionary<ReductionDeclaration, MethodInfo> lookup = new Dictionary<ReductionDeclaration, MethodInfo>(){
            {PrintString, typeof(Console).GetMethod("WriteLine", new[]{typeof(string)})}
        };

        public static IEnumerable<ReductionDeclaration> All
        {
            get
            {
                return lookup.Keys;
            }
        }

        public static MethodInfo DotNetFunctionForBuiltin(ReductionDeclaration fn)
        {
            MethodInfo result = null;
            if (!lookup.TryGetValue(fn, out result)){
                return null;
            }

            return result;
        }
    }
}
