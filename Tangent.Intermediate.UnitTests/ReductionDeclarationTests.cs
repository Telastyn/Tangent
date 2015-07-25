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

        [TestMethod]
        public void GenericSpecialization()
        {
            var genericParam = new ParameterDeclaration("T", TangentType.Any.Kind);
            var inference = GenericInferencePlaceholder.For(genericParam);
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", TangentType.Int), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", inference), new Function(TangentType.Void, null));

            Assert.IsTrue(fn1.IsSpecializationOf(fn2));
            Assert.IsFalse(fn2.IsSpecializationOf(fn1));
        }


        [TestMethod]
        public void NestedGenericSpecialization()
        {
            var genericParam = new ParameterDeclaration("T", TangentType.Any.Kind);
            var inference = GenericInferencePlaceholder.For(genericParam);
            var listTsT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var listT = new TypeDeclaration(new[] { new PhrasePart("List"), new PhrasePart(listTsT) }, new ProductType(new PhrasePart[0]));
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", BoundGenericType.For(listT, new[] { TangentType.Int })), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", BoundGenericType.For(listT, new[] { inference })), new Function(TangentType.Void, null));

            Assert.IsTrue(fn1.IsSpecializationOf(fn2));
            Assert.IsFalse(fn2.IsSpecializationOf(fn1));
        }

        [TestMethod]
        public void NestedGenericPartialSpecialization()
        {
            var genericParam1 = new ParameterDeclaration("K", TangentType.Any.Kind);
            var inference1 = GenericInferencePlaceholder.For(genericParam1);
            var genericParam2 = new ParameterDeclaration("V", TangentType.Any.Kind);
            var inference2 = GenericInferencePlaceholder.For(genericParam2);
            var genericParam3 = new ParameterDeclaration("V", TangentType.Any.Kind);
            var inference3 = GenericInferencePlaceholder.For(genericParam3);
            var dictKV = new TypeDeclaration(new[] { new PhrasePart("Dict"), new PhrasePart(new ParameterDeclaration("K", TangentType.Any.Kind)), new PhrasePart(new ParameterDeclaration("V", TangentType.Any.Kind)) }, new ProductType(new PhrasePart[0]));
            var fn1 = new ReductionDeclaration(new ParameterDeclaration("x", BoundGenericType.For(dictKV, new[] { TangentType.Int, inference3 })), new Function(TangentType.Void, null));
            var fn2 = new ReductionDeclaration(new ParameterDeclaration("x", BoundGenericType.For(dictKV, new[] { inference1, inference2 })), new Function(TangentType.Void, null));

            Assert.IsTrue(fn1.IsSpecializationOf(fn2));
            Assert.IsFalse(fn2.IsSpecializationOf(fn1));
        }
    }
}
