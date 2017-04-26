using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate.Interop
{
    public static class BuiltinFunctions
    {
        public static ReductionDeclaration PrintString = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("s", TangentType.String) }, new Function(TangentType.Void, null));
        public static ReductionDeclaration PrintInt = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("x", TangentType.Int) }, new Function(TangentType.Void, null));
        public static ReductionDeclaration PrintDouble = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("x", TangentType.Double) }, new Function(TangentType.Void, null));
        public static ReductionDeclaration PrintBool = new ReductionDeclaration(new PhrasePart[] { new Identifier("print"), new ParameterDeclaration("x", TangentType.Bool) }, new Function(TangentType.Void, null));

        public static ReductionDeclaration AddInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("+"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Add, TangentType.Int));
        public static ReductionDeclaration SubInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("-"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Sub, TangentType.Int));
        public static ReductionDeclaration MulInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("*"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Mul, TangentType.Int));
        public static ReductionDeclaration DivInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("/"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Div, TangentType.Int));
        public static ReductionDeclaration ModInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("%"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Rem, TangentType.Int));
        public static ReductionDeclaration EqInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("="), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Ceq, TangentType.Bool));
        public static ReductionDeclaration GtInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier(">"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Cgt, TangentType.Bool));
        public static ReductionDeclaration LtInt = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Int), new Identifier("<"), new ParameterDeclaration("b", TangentType.Int) }, new DirectOpCode(OpCodes.Clt, TangentType.Bool));

        public static ReductionDeclaration AndBool = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Bool), new Identifier("and"), new ParameterDeclaration("b", TangentType.Bool) }, new DirectOpCode(OpCodes.And, TangentType.Bool));
        public static ReductionDeclaration OrBool = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Bool), new Identifier("or"), new ParameterDeclaration("b", TangentType.Bool) }, new DirectOpCode(OpCodes.Or, TangentType.Bool));
        public static ReductionDeclaration EqBool = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Bool), new Identifier("="), new ParameterDeclaration("b", TangentType.Bool) }, new DirectOpCode(OpCodes.Ceq, TangentType.Bool));

        public static ReductionDeclaration AddDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier("+"), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Add, TangentType.Double));
        public static ReductionDeclaration SubDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier("-"), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Sub, TangentType.Double));
        public static ReductionDeclaration MulDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier("*"), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Mul, TangentType.Double));
        public static ReductionDeclaration EqDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier("="), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Ceq, TangentType.Bool));
        public static ReductionDeclaration GtDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier(">"), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Cgt, TangentType.Bool));
        public static ReductionDeclaration LtDouble = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", TangentType.Double), new Identifier("<"), new ParameterDeclaration("b", TangentType.Double) }, new DirectOpCode(OpCodes.Clt, TangentType.Bool));

        private static ParameterDeclaration EqGenericParameter = new ParameterDeclaration("T", TangentType.Any.Kind);
        public static ReductionDeclaration EqGeneric = new ReductionDeclaration(new PhrasePart[] { new ParameterDeclaration("a", GenericArgumentReferenceType.For(EqGenericParameter)), new Identifier("="), new ParameterDeclaration("b", GenericArgumentReferenceType.For(EqGenericParameter)) }, new DirectOpCode(OpCodes.Ceq, TangentType.Bool));

        private static readonly Dictionary<ReductionDeclaration, MethodInfo> lookup = new Dictionary<ReductionDeclaration, MethodInfo>(){
            {PrintString, typeof(Console).GetMethod("WriteLine", new[]{typeof(string)})},
            {PrintInt, typeof(Console).GetMethod("WriteLine", new[]{typeof(int)})},
            {PrintDouble, typeof(Console).GetMethod("WriteLine", new[]{typeof(double)})},
            {PrintBool, typeof(Console).GetMethod("WriteLine", new[]{typeof(bool)})},
        };

        public static IEnumerable<ReductionDeclaration> AsmFunctions = new List<ReductionDeclaration>()
        {
            AddInt,
            SubInt,
            MulInt,
            //EqInt,
            GtInt,
            LtInt,

            AndBool,
            OrBool,
            //EqBool,

            AddDouble,
            SubDouble,
            MulDouble,
            //EqDouble,
            GtDouble,
            LtDouble,

            EqGeneric
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
            if (!lookup.TryGetValue(fn, out result)) {
                return null;
            }

            return result;
        }
    }
}
