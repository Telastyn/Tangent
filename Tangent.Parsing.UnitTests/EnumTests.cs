using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    public class EnumTests
    {
        // enum { id+, id+, ... }
        [TestMethod]
        public void HappyPath()
        {
            var test = "enum { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            int takes;
            var result = Grammar.EnumImpl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            var theEnum = result.Result as EnumType;
            Assert.AreEqual(2, theEnum.Values.Count());
            Assert.AreEqual("a", theEnum.Values.First().Value);
            Assert.AreEqual("b", theEnum.Values.Skip(1).First().Value);
        }

        [TestMethod]
        public void EnumFails()
        {
            var test = "num { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            int takes;
            var result = Grammar.EnumImpl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingOpenBracket()
        {
            var test = "enum a, b";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            int takes;
            var result = Grammar.EnumImpl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingClose()
        {
            var test = "enum { a, b ";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            int takes;
            var result = Grammar.EnumImpl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void EnumCase()
        {
            var test = "ENUM { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            int takes;
            var result = Grammar.EnumImpl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }
    }
}
