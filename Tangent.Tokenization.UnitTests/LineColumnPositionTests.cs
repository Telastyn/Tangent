using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Tokenization.UnitTests
{

    [TestClass]
    public class LineColumnPositionTests
    {

        [TestMethod]
        public void BasicHappyPath()
        {
            string test = "How Now Brown Cow";
            LineColumnPosition result = LineColumnPosition.Create(test, 7);

            Assert.AreEqual(8, result.Column);
            Assert.AreEqual(1, result.Line);
        }

        [TestMethod]
        public void NewLinedHappyPath()
        {
            string test = "How\nNow\nBrown\nCow\n";
            LineColumnPosition result = LineColumnPosition.Create(test, 6);

            Assert.AreEqual(2, result.Line);
            Assert.AreEqual(3, result.Column);
        }

        [TestMethod]
        public void IndexIsNewLine()
        {
            string test = "How\nNow\nBrown\nCow\n";
            LineColumnPosition result = LineColumnPosition.Create(test, 7);

            Assert.AreEqual(2, result.Line);
            Assert.AreEqual(4, result.Column);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SmallIndex()
        {
            string test = "How\nNow\nBrown\nCow\n";
            LineColumnPosition result = LineColumnPosition.Create(test, -7); // boom.
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BigIndex()
        {
            string test = "How\nNow\nBrown\nCow\n";
            LineColumnPosition result = LineColumnPosition.Create(test, 42); // boom.
        }
    }
}
