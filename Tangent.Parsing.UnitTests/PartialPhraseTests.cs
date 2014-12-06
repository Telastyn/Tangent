using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests {
    [TestClass]
    public class PartialPhraseTests {
        
        [TestMethod]
        public void BasicPath() {
            var test = "(x: int) plus (y: int)";
            var tokens = Tokenize.ProgramFile(test).ToList();

            var result = Parse.PartialPhrase(tokens);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.AreEqual("x", x.Parameter.Takes.Value);
            Assert.IsTrue(x.Parameter.Returns.IsEndResult);
            Assert.AreEqual(1, x.Parameter.Returns.Result.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Result[0]);

            Assert.AreEqual("plus", plus.Identifier.Value);
        }

        [TestMethod]
        public void BasicPathAvoidsRemainingTokens() {
            var test = "(x: int) plus (y: int) :>";
            var tokens = Tokenize.ProgramFile(test).ToList();

            var result = Parse.PartialPhrase(tokens);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, tokens.Count);
        }

        [TestMethod]
        public void MissingParensIsFailure() {
            var test = "(x: int) plus (y: int :>";
            var tokens = Tokenize.ProgramFile(test).ToList();

            var result = Parse.PartialPhrase(tokens);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void BadInput() {
            var test = ":> (x: int) plus (y: int)";
            var tokens = Tokenize.ProgramFile(test).ToList();

            var result = Parse.PartialPhrase(tokens);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void LongPhrasePart() {
            var test = "(x y z: unsigned int) plus (ab c: int)";
            var tokens = Tokenize.ProgramFile(test).ToList();

            var result = Parse.PartialPhrase(tokens);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.AreEqual("x", x.Parameter.Takes.Value);
            Assert.IsTrue(x.Parameter.Returns.IsReductionRule);
            Assert.AreEqual("y", x.Parameter.Returns.Rule.Takes);
            Assert.IsTrue(x.Parameter.Returns.Rule.Returns.IsReductionRule);
            Assert.AreEqual("z", x.Parameter.Returns.Rule.Returns.Rule.Takes);
            Assert.IsTrue(x.Parameter.Returns.Rule.Returns.Rule.Returns.IsEndResult);
            Assert.AreEqual(2, x.Parameter.Returns.Rule.Returns.Rule.Returns.Result.Count);
            Assert.AreEqual("unsigned", x.Parameter.Returns.Rule.Returns.Rule.Returns.Result[0]);
            Assert.AreEqual("int", x.Parameter.Returns.Rule.Returns.Rule.Returns.Result[1]);
        }
    }
}
