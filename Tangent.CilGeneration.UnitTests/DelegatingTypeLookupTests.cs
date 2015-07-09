using System;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;

namespace Tangent.CilGeneration.UnitTests
{
    [TestClass]
    public class DelegatingTypeLookupTests
    {
        [TestMethod]
        public void VoidMakesVoid()
        {
            var mockCompiler = new Mock<ITypeCompiler>();
            var lookup = new DelegatingTypeLookup(mockCompiler.Object, Enumerable.Empty<TypeDeclaration>());

            var result = lookup[TangentType.Void];
            Assert.AreEqual(typeof(void), result);
        }

        [TestMethod]
        public void LookupCompilesOnce()
        {
            var typeDecl = new TypeDeclaration("foo", new EnumType(new Identifier[] { "bar" }));
            int times = 0;
            var mockCompiler = new Mock<ITypeCompiler>();
            mockCompiler.Setup(c => c.Compile(typeDecl, It.IsAny<Action<TangentType, Type>>(), It.IsAny<Func<TangentType, bool, Type>>())).Returns(typeof(DateTimeOffset)).Callback(() => times++);
            var lookup = new DelegatingTypeLookup(mockCompiler.Object, new[] { typeDecl });

            var result = lookup[typeDecl.Returns];
            Assert.AreEqual(typeof(DateTimeOffset), result);
            result = lookup[typeDecl.Returns];
            Assert.AreEqual(1, times);
        }

        [TestMethod]
        public void LazyVoidWorks()
        {
            var mockCompiler = new Mock<ITypeCompiler>();
            var lookup = new DelegatingTypeLookup(mockCompiler.Object, Enumerable.Empty<TypeDeclaration>());
            var result = lookup[TangentType.Void.Lazy];

            Assert.AreEqual(typeof(Action), result);
        }

        [TestMethod]
        public void LazyFooWorks()
        {
            var typeDecl = new TypeDeclaration("foo", new EnumType(new Identifier[] { "bar" }));
            var mockCompiler = new Mock<ITypeCompiler>();
            mockCompiler.Setup(c => c.Compile(typeDecl, It.IsAny<Action<TangentType, Type>>(), It.IsAny<Func<TangentType, bool, Type>>())).Returns(typeof(DateTimeOffset));
            var lookup = new DelegatingTypeLookup(mockCompiler.Object, new[] { typeDecl });

            var result = lookup[typeDecl.Returns.Lazy];
            Assert.AreEqual(typeof(Func<DateTimeOffset>), result);
        }

        [TestMethod]
        public void SuperLazyFooWorks()
        {
            var typeDecl = new TypeDeclaration("foo", new EnumType(new Identifier[] { "bar" }));
            var mockCompiler = new Mock<ITypeCompiler>();
            mockCompiler.Setup(c => c.Compile(typeDecl, It.IsAny<Action<TangentType, Type>>(), It.IsAny<Func<TangentType, bool, Type>>())).Returns(typeof(DateTimeOffset));
            var lookup = new DelegatingTypeLookup(mockCompiler.Object, new[] { typeDecl });

            var result = lookup[typeDecl.Returns.Lazy.Lazy];
            Assert.AreEqual(typeof(Func<Func<DateTimeOffset>>), result);
        }
    }
}
