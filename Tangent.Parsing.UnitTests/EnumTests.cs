using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    public class EnumTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var test = "enum { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            var result = Parse.Enum(tokens.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Result.Values.Count());
            Assert.AreEqual("a", result.Result.Values.First().Value);
            Assert.AreEqual("b", result.Result.Values.Skip(1).First().Value);
        }

        [TestMethod]
        public void EnumFails()
        {
            var test = "num { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            var result = Parse.Enum(tokens.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingOpenBracket()
        {
            var test = "enum a, b";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            var result = Parse.Enum(tokens.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingClose()
        {
            var test = "enum { a, b ";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            var result = Parse.Enum(tokens.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void EnumCase()
        {
            var test = "ENUM { a, b }";
            var tokens = Tokenize.ProgramFile(test, "test.tan");

            var result = Parse.Enum(tokens.ToList());

            Assert.IsFalse(result.Success);
        }
    }
}
