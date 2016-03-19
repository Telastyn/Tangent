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
    public class DelimitedParserTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var tokens = Tokenize.ProgramFile("42:2:6", "test");
            var parser = Parser.Delimited(LiteralParser.Colon, TestIntParser.Common);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, taken);
            Assert.IsTrue(new[] { 42, 2, 6 }.SequenceEqual(result.Result));
        }

        [TestMethod]
        public void SingleElementWorks()
        {
            var tokens = Tokenize.ProgramFile("42", "test");
            var parser = Parser.Delimited(LiteralParser.Colon, TestIntParser.Common);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, taken);
            Assert.IsTrue(new[] { 42 }.SequenceEqual(result.Result));
        }

        [TestMethod]
        public void BadElementFails()
        {
            var tokens = Tokenize.ProgramFile("foo", "test");
            var parser = Parser.Delimited(LiteralParser.Colon, TestIntParser.Common);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void HappyPathWithTrailer()
        {
            var tokens = Tokenize.ProgramFile("42:2:6:", "test");
            var parser = Parser.Delimited(LiteralParser.Colon, TestIntParser.Common, true, true);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(6, taken);
            Assert.IsTrue(new[] { 42, 2, 6 }.SequenceEqual(result.Result));
        }

        [TestMethod]
        public void RequiresOneHappyPath()
        {
            var tokens = Tokenize.ProgramFile("", "test");
            var parser = Parser.Delimited(LiteralParser.Colon, TestIntParser.Common, false, false);
            int taken;
            var result = parser.Parse(tokens, out taken);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, taken);
            Assert.IsTrue(new int[] {  }.SequenceEqual(result.Result));
        }
    }
}
