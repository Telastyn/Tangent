using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests {

    [TestClass]
    public class PartialBlockParseTests {

        [TestMethod]
        public void MissingOpenDoesNotMatch() {
            var test = Tokenize.ProgramFile("x");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingCloseDoesNotMatch() {
            var test = Tokenize.ProgramFile("{x;");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void EmptyBlocksAreFine() {
            var test = Tokenize.ProgramFile("{}");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BlocksAreFine() {
            var test = Tokenize.ProgramFile("{x;}");

            var result = Parse.PartialBlock(test.ToList());

            Assert.IsTrue(result.Success);
        }
    }
}
