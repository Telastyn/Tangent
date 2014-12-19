using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests
{

    [TestClass]
    public class PartialStatementParseTests
    {

        [TestMethod]
        public void HappyPath()
        {
            var test = Tokenize.ProgramFile("a b c;");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Result.Count());
            Assert.IsTrue(new List<Identifier>() { "a", "b", "c" }.SequenceEqual(result.Result));
        }

        [TestMethod]
        public void UnexpectedSymbol()
        {
            var test = Tokenize.ProgramFile("a + c;");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void StopAtSemi()
        {
            var test = Tokenize.ProgramFile("a b c; +");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void MissingSemiErrors()
        {
            var test = Tokenize.ProgramFile("a b c");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsFalse(result.Success);
        }
    }
}
