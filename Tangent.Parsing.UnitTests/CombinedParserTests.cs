using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CombinedParserTests
    {
        [TestMethod]
        public void SelectingParserHappyPath()
        {
            var tokens = Tokenize.ProgramFile("6", "test");
            var parser = TestIntParser.Common.Select(x => x + 2);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(8, result.Result);
            Assert.AreEqual(1, taken);
        }

        [TestMethod]
        public void CombineParser1()
        {
            var tokens = Tokenize.ProgramFile("6 2", "test");
            var parser = Parser.Combine(TestIntParser.Common, TestIntParser.Common, (a, b) => a - b);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.Result);
            Assert.AreEqual(2, taken);
        }

        [TestMethod]
        public void CombineParser2()
        {
            var tokens = Tokenize.ProgramFile("6 2 5", "test");
            var parser = Parser.Combine(TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, (a, b, c) => (a - b) * c);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(20, result.Result);
            Assert.AreEqual(3, taken);
        }

        [TestMethod]
        public void CombineParser3()
        {
            var tokens = Tokenize.ProgramFile("6 2 5 3", "test");
            var parser = Parser.Combine(TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, (a, b, c, d) => ((a - b) * c) + d);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(23, result.Result);
            Assert.AreEqual(4, taken);
        }

        [TestMethod]
        public void CombineParser4()
        {
            var tokens = Tokenize.ProgramFile("6 2 5 3 16", "test");
            var parser = Parser.Combine(TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, TestIntParser.Common, (a, b, c, d, e) => (((a - b) * c) + d) - e);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(7, result.Result);
            Assert.AreEqual(5, taken);
        }
    }
}
