using System;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using System.Collections.Generic;

namespace Tangent.CilGeneration.UnitTests
{
    [TestClass]
    public class DelegatingTypeLookupTests
    {
        private static readonly object sync = new object();

        [TestMethod]
        public void VoidMakesVoid()
        {
            lock (sync) {
                var mockCompiler = new Mock<ITypeCompiler>();
                using (var lookup = new DelegatingTypeLookup(mockCompiler.Object, Enumerable.Empty<TypeDeclaration>(), AppDomain.CurrentDomain)) {

                    var result = lookup[TangentType.Void];
                    Assert.AreEqual(typeof(void), result);
                }
            }
        }

        [TestMethod]
        public void LookupCompilesOnce()
        {
            lock (sync) {
                var typeDecl = new TypeDeclaration("foo", new EnumType(new Identifier[] { "bar" }));
                int times = 0;
                var mockCompiler = new Mock<ITypeCompiler>();
                mockCompiler.Setup(c => c.Compile(typeDecl, It.IsAny<Action<TangentType, Type>>(), It.IsAny<Func<TangentType, bool, Type>>())).Returns(typeof(DateTimeOffset)).Callback(() => times++);
                using (var lookup = new DelegatingTypeLookup(mockCompiler.Object, new[] { typeDecl }, AppDomain.CurrentDomain)) {

                    var result = lookup[typeDecl.Returns];
                    Assert.AreEqual(typeof(DateTimeOffset), result);
                    result = lookup[typeDecl.Returns];
                    Assert.AreEqual(1, times);
                }
            }
        }
    }
}
