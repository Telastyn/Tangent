using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate
{
    public static class BuiltinFunctions
    {
        public static ReductionDeclaration PrintString = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("s", TangentType.String) }, new Function(TangentType.Void, null));
        public static ReductionDeclaration PrintInt = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("x", TangentType.Int) }, new Function(TangentType.Void, null));
        public static ReductionDeclaration AddInt = new ReductionDeclaration(new PhrasePart[] { new Identifier("asm"), new Identifier("add"), new ParameterDeclaration("a", TangentType.Int), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Add, TangentType.Int));

        private static readonly Dictionary<ReductionDeclaration, MethodInfo> lookup = new Dictionary<ReductionDeclaration, MethodInfo>(){
            {PrintString, typeof(Console).GetMethod("WriteLine", new[]{typeof(string)})},
            {PrintInt, typeof(Console).GetMethod("WriteLine", new[]{typeof(int)})}
        };

        public static IEnumerable<ReductionDeclaration> AsmFunctions = new List<ReductionDeclaration>()
        {
            AddInt
        };

        public static IEnumerable<ReductionDeclaration> All
        {
            get
            {
                return lookup.Keys.Concat(AsmFunctions).ToList();
            }
        }

        public static MethodInfo DotNetFunctionForBuiltin(ReductionDeclaration fn)
        {
            MethodInfo result = null;
            if (!lookup.TryGetValue(fn, out result))
            {
                return null;
            }

            return result;
        }
    }
}
