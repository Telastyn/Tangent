using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Intermediate.UnitTests
{

    [TestClass]
    public class IdentifierTests
    {

        [TestMethod]
        public void ValueEqualityWorks()
        {
            Assert.IsTrue(new Identifier("foo").Equals(new Identifier("foo")));
        }

        [TestMethod]
        public void ValueEqualityWorksWithOperator()
        {
            Assert.IsTrue(new Identifier("foo") == new Identifier("foo"));
        }

        [TestMethod]
        public void ValueInequalityWorksWithOperator()
        {
            Assert.IsTrue(new Identifier("foo") != new Identifier("foot"));
        }

        [TestMethod]
        public void NullValueEqualityWorksWithOperator()
        {
            Assert.IsTrue((Identifier)null == (Identifier)null);
        }

        [TestMethod]
        public void NullValueInequalityWorksWithOperator()
        {
            Assert.IsTrue(null != new Identifier("foot"));
        }

        [TestMethod]
        public void NullValueInequalityWorksWithOperator2()
        {
            Assert.IsTrue(new Identifier("foot") != null);
        }

        [TestMethod]
        public void ValueEqualityWorksWithNull()
        {
            Assert.IsFalse(new Identifier("foo").Equals(null));
        }

        [TestMethod]
        public void ValueEqualityWorksWithRandomCrap()
        {
            Assert.IsFalse(new Identifier("foo").Equals(12));
        }

        [TestMethod]
        public void ImplicitConversionExists()
        {
            Assert.IsTrue(new Identifier("foo") == "foo");
        }
    }
}
