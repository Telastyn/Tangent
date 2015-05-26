using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    public class ReductionDeclarationTests
    {
        [TestMethod]
        public void SameFunctionFalse()
        {
            Assert.IsFalse(BuiltinFunctions.PrintInt.IsSpecializationOf(BuiltinFunctions.PrintInt));
        }

        [TestMethod]
        public void DisjointFunctionFalse()
        {
            Assert.IsFalse(BuiltinFunctions.PrintInt.IsSpecializationOf(BuiltinFunctions.EqBool));
        }

        [TestMethod]
        public void ReturnTypeMatters()
        {
            var fn1 = new ReductionDeclaration("foo", new Function(TangentType.Int, null));
            var fn2 = new ReductionDeclaration("foo", new Function(TangentType.Void, null));

            Assert.IsFalse(fn1.IsSpecializationOf(fn2));
        }

        [TestMethod]
        public void HappyPathSpecialization()
        {
            var testType = new EnumType(new List<Identifier>() { "foo", "bar" });
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", testType), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", testType.SingleValueTypeFor("foo")), new Function(TangentType.Void, null));

            Assert.IsTrue(fn2.IsSpecializationOf(fn1));
        }

        [TestMethod]
        public void SpecializationIsDirectional()
        {
            var testType = new EnumType(new List<Identifier>() { "foo", "bar" });
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", testType), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", testType.SingleValueTypeFor("foo")), new Function(TangentType.Void, null));

            Assert.IsFalse(fn1.IsSpecializationOf(fn2));
        }

        [TestMethod]
        public void SpecializationIgnoresParamNames()
        {
            var testType = new EnumType(new List<Identifier>() { "foo", "bar" });
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", testType), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("y", testType.SingleValueTypeFor("foo")), new Function(TangentType.Void, null));

            Assert.IsTrue(fn2.IsSpecializationOf(fn1));
        }

        [TestMethod]
        public void SumTypeSpecialization()
        {
            var testType = SumType.For(new[] { TangentType.Int, TangentType.String });
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", testType), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", TangentType.Int), new Function(TangentType.Void, null));

            Assert.IsTrue(fn2.IsSpecializationOf(fn1));
        }

        [TestMethod]
        public void SumTypeSpecializationIsDirectional()
        {
            var testType = SumType.For(new[] { TangentType.Int, TangentType.String });
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", testType), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", TangentType.Int), new Function(TangentType.Void, null));

            Assert.IsFalse(fn1.IsSpecializationOf(fn2));
        }
    }
}
