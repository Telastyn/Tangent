using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    public class TransformationOrderingTests
    {
        [TestMethod]
        public void TypeIsPreferred()
        {
            var a = new Mock<TransformationRule>();
            a.Setup(x => x.MaxTakeCount).Returns(1);
            a.Setup(x => x.Type).Returns(TransformationType.BuiltIn);

            var b = new Mock<TransformationRule>();
            b.Setup(x => x.MaxTakeCount).Returns(1);
            b.Setup(x => x.Type).Returns(TransformationType.Function);

            var c = a.Object;
            var d = b.Object;

            var result = new[] { c, d }.Sort();
            Assert.IsTrue(result.SequenceEqual(new[] { c, d }));

            result = new[] { d, c }.Sort();
            Assert.IsTrue(result.SequenceEqual(new[] { c, d }));
        }

        [TestMethod]
        public void LongTakesArePreferred()
        {
            var a = new Mock<TransformationRule>();
            a.Setup(x => x.MaxTakeCount).Returns(2);
            a.Setup(x => x.Type).Returns(TransformationType.BuiltIn);

            var b = new Mock<TransformationRule>();
            b.Setup(x => x.MaxTakeCount).Returns(1);
            b.Setup(x => x.Type).Returns(TransformationType.BuiltIn);

            var c = a.Object;
            var d = b.Object;

            var result = new[] { c, d }.Sort();
            Assert.IsTrue(result.SequenceEqual(new[] { c, d }));

            result = new[] { d, c }.Sort();
            Assert.IsTrue(result.SequenceEqual(new[] { c, d }));
        }
    }
}
