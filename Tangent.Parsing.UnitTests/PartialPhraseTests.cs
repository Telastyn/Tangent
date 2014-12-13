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

            Assert.AreEqual("x", x.Parameter.Takes.First().Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns[0]);

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

            Assert.AreEqual(3, x.Parameter.Takes.Count);
            Assert.IsTrue(new[] { "x", "y", "z" }.SequenceEqual(x.Parameter.Takes.Select(id=>id.Value)));

            Assert.AreEqual(2, x.Parameter.Returns.Count);
            Assert.IsTrue(new[] { "unsigned", "int" }.SequenceEqual(x.Parameter.Returns.Select(id => id.Value)));
        }
    }
}
