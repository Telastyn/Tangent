﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using System.Linq;
using System.Reflection.Emit;
using Moq;

namespace Tangent.CilGeneration.UnitTests
{
    [TestClass]
    public class CilScopeTests
    {
        private static ITypeLookup emptyTypeLookup;

        static CilScopeTests()
        {
            var mock = new Mock<ITypeLookup>();
            mock.Setup(tl => tl[TangentType.Void]).Returns(typeof(void));
            emptyTypeLookup = mock.Object;
        }

        [TestMethod]
        public void BasicMethodHappyPath()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("BasicMethodHappyPath"), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("BasicMethodHappyPath");
            var t = module.DefineType("BasicMethodHappyPath");
            var fn = new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())));
            var scope = new CilScope(t, new[] { fn }, emptyTypeLookup);

            scope.Compile(new CilFunctionCompiler(EmptyFunctionLookup.Common));
            t.CreateType();

            var stub = scope[fn];
            Assert.IsNotNull(stub);
            Assert.AreEqual(0, stub.GetParameters().Length);
            Assert.AreEqual(typeof(void), stub.ReturnType);
        }

        [TestMethod]
        public void MethodWithParamsHappyPath()
        {
            var enumT = new EnumType(new Identifier[] { "a", "b" });
            var mockLookup = new Mock<ITypeLookup>();
            mockLookup.Setup(tl => tl[It.IsAny<TangentType>()]).Returns(typeof(DateTimeOffset));
            mockLookup.Setup(tl => tl[TangentType.Void]).Returns(typeof(void));

            var assembly = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("MethodWithParamsHappyPath"), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("MethodWithParamsHappyPath");
            var t = module.DefineType("MethodWithParamsHappyPath");
            var fn = new ReductionDeclaration(new PhrasePart[] { new PhrasePart("foo"), new PhrasePart(new ParameterDeclaration("bar", enumT)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())));
            var scope = new CilScope(t, new[] { fn }, mockLookup.Object);

            scope.Compile(new CilFunctionCompiler(EmptyFunctionLookup.Common));
            t.CreateType();

            var stub = scope[fn];
            Assert.IsNotNull(stub);
            Assert.AreEqual(1, stub.GetParameters().Length);
            Assert.AreEqual(typeof(DateTimeOffset), stub.GetParameters().First().ParameterType);
            Assert.AreEqual(typeof(void), stub.ReturnType);
        }

        [TestMethod]
        public void MethodSpecializationHappyPath()
        {
            var enumT = new EnumType(new Identifier[] { "a", "b" });
            var mockLookup = new Mock<ITypeLookup>();
            mockLookup.Setup(tl => tl[It.IsAny<TangentType>()]).Returns(typeof(DateTimeOffset));
            mockLookup.Setup(tl => tl[TangentType.Void]).Returns(typeof(void));

            var assembly = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("MethodWithParamsHappyPath"), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("MethodWithParamsHappyPath");
            var t = module.DefineType("MethodWithParamsHappyPath");
            var fn = new ReductionDeclaration(new PhrasePart[] { new PhrasePart("foo"), new PhrasePart(new ParameterDeclaration("bar", enumT)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())));
            var fn2 = new ReductionDeclaration(new PhrasePart[] { new PhrasePart("foo"), new PhrasePart(new ParameterDeclaration("bar", new SingleValueType(enumT, "b"))) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())));
            var scope = new CilScope(t, new[] { fn, fn2 }, mockLookup.Object);

            scope.Compile(new CilFunctionCompiler(EmptyFunctionLookup.Common));
            t.CreateType();

            var realfns = t.GetMethods().Where(mi => mi.Name.StartsWith("foo"));
            Assert.AreEqual(2, realfns.Count());
            Assert.IsTrue(realfns.All(f => f.GetParameters().Count() == 1 && f.GetParameters().First().ParameterType == typeof(DateTimeOffset)));

            var stub = scope[fn];
            var stub2 = scope[fn2];
            Assert.IsNotNull(stub);
            Assert.AreEqual(typeof(void), stub.ReturnType);

            Assert.IsNotNull(stub2);
            Assert.AreEqual(typeof(void), stub2.ReturnType);

            Assert.IsFalse(object.ReferenceEquals(stub, stub2));
        }
    }
}
