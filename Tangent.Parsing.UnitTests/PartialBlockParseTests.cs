using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests
{

    [TestClass]
    public class PartialBlockParseTests
    {

        [TestMethod]
        public void MissingOpenDoesNotMatch()
        {
            var test = Tokenize.ProgramFile("x", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingCloseDoesNotMatch()
        {
            var test = Tokenize.ProgramFile("{x;", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void EmptyBlocksAreFine()
        {
            var test = Tokenize.ProgramFile("{}", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BlocksAreFine()
        {
            var test = Tokenize.ProgramFile("{x;}", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void LastSemiIsOptional()
        {
            var test = Tokenize.ProgramFile("{x}", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void LastSemiIsOptional2()
        {
            var test = Tokenize.ProgramFile("{a; b; c d e f; x}", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ParenMissingCloseDoesNotMatch()
        {
            var test = Tokenize.ProgramFile("(x;", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ParenEmptyBlocksAreFine()
        {
            var test = Tokenize.ProgramFile("()", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ParenBlocksAreFine()
        {
            var test = Tokenize.ProgramFile("(x;)", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ParenLastSemiIsOptional()
        {
            var test = Tokenize.ProgramFile("(x)", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ParenLastSemiIsOptional2()
        {
            var test = Tokenize.ProgramFile("(a; b; c d e f; x)", "test.tan");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }
    }
}
