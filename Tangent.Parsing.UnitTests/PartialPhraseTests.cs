using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    public class PartialPhraseTests
    {

        [TestMethod]
        public void BasicPath()
        {
            var test = "(x: int) plus (y: int)";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("x", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("plus", plus.Identifier.Value);
        }

        [TestMethod]
        public void BasicThisPath()
        {
            var test = "(this) plus (y: int)";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, true);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("this", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("this", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("plus", plus.Identifier.Value);
        }

        [TestMethod]
        public void BasicSymbolPath()
        {
            var test = "(x: int)+(y: int)";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("x", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("+", plus.Identifier.Value);
        }

        [TestMethod]
        public void BasicPathAvoidsRemainingTokens()
        {
            var test = "(x: int) plus (y: int) :>";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, tokens.Count);
        }

        [TestMethod]
        public void MissingParensIsFailure()
        {
            var test = "(x: int) plus (y: int :>";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void BadInput()
        {
            var test = ":> (x: int) plus (y: int)";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void LongPhrasePart()
        {
            var test = "(x y z: unsigned int) plus (ab c: int)";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

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
            Assert.IsTrue(new[] { "x", "y", "z" }.SequenceEqual(x.Parameter.Takes.Select(id => id.Identifier.Value)));

            Assert.AreEqual(2, x.Parameter.Returns.Count);
            Assert.IsTrue(new[] { "unsigned", "int" }.SequenceEqual(x.Parameter.Returns.Cast<IdentifierExpression>().Select(id => id.Identifier.Value)));
        }

        [TestMethod]
        public void BasicInferencePath()
        {
            var test = "(x: int) plus (y: (T:any))";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("x", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("plus", plus.Identifier.Value);

            Assert.IsTrue(y.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("y", y.Parameter.Takes.First().Identifier.Value);
            var inference = y.Parameter.Returns.First();
            Assert.AreEqual(ExpressionNodeType.TypeInference, inference.NodeType);
            var castInference = (PartialTypeInferenceExpression)inference;
            Assert.AreEqual(1, castInference.InferenceName.Count());
            Assert.AreEqual("T", castInference.InferenceName.First().Value);
            Assert.AreEqual(1, castInference.InferenceExpression.Count());
            Assert.AreEqual(ExpressionNodeType.Identifier, castInference.InferenceExpression.First().NodeType);
            Assert.AreEqual("any", castInference.InferenceExpression.Cast<IdentifierExpression>().First().Identifier.Value);
        }

        [TestMethod]
        public void NestedInferencePath()
        {
            var test = "(x: int) plus (y: foo(T:any))";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("x", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("plus", plus.Identifier.Value);

            Assert.IsTrue(y.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("y", y.Parameter.Takes.First().Identifier.Value);
            var typeExpr = y.Parameter.Returns;
            Assert.AreEqual(2, typeExpr.Count);
            Assert.AreEqual(ExpressionNodeType.Identifier, typeExpr[0].NodeType);
            Assert.AreEqual("foo", ((IdentifierExpression)typeExpr[0]).Identifier.Value);
            Assert.AreEqual(ExpressionNodeType.TypeInference, typeExpr[1].NodeType);
            var inference = y.Parameter.Returns[1];
            Assert.AreEqual(ExpressionNodeType.TypeInference, inference.NodeType);
            var castInference = (PartialTypeInferenceExpression)inference;
            Assert.AreEqual(1, castInference.InferenceName.Count());
            Assert.AreEqual("T", castInference.InferenceName.First().Value);
            Assert.AreEqual(1, castInference.InferenceExpression.Count());
            Assert.AreEqual(ExpressionNodeType.Identifier, castInference.InferenceExpression.First().NodeType);
            Assert.AreEqual("any", castInference.InferenceExpression.Cast<IdentifierExpression>().First().Identifier.Value);
        }

        [TestMethod]
        public void ImpliedAnyInferencePath()
        {
            var test = "(x: int) plus (y: foo(T))";
            var tokens = Tokenize.ProgramFile(test, "test.tan").ToList();

            var result = Parse.PartialPhrase(tokens, false);

            Assert.IsTrue(result.Success);
            var partialPhrase = result.Result;
            Assert.AreEqual(3, partialPhrase.Count);
            var x = partialPhrase[0];
            var plus = partialPhrase[1];
            var y = partialPhrase[2];

            Assert.IsFalse(x.IsIdentifier);
            Assert.IsTrue(plus.IsIdentifier);
            Assert.IsFalse(y.IsIdentifier);

            Assert.IsTrue(x.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("x", x.Parameter.Takes.First().Identifier.Value);
            Assert.AreEqual(1, x.Parameter.Takes.Count);
            Assert.AreEqual(1, x.Parameter.Returns.Count);
            Assert.AreEqual("int", x.Parameter.Returns.Cast<IdentifierExpression>().First().Identifier.Value);

            Assert.AreEqual("plus", plus.Identifier.Value);

            Assert.IsTrue(y.Parameter.Takes.First().IsIdentifier);
            Assert.AreEqual("y", y.Parameter.Takes.First().Identifier.Value);
            var typeExpr = y.Parameter.Returns;
            Assert.AreEqual(2, typeExpr.Count);
            Assert.AreEqual(ExpressionNodeType.Identifier, typeExpr[0].NodeType);
            Assert.AreEqual("foo", ((IdentifierExpression)typeExpr[0]).Identifier.Value);
            Assert.AreEqual(ExpressionNodeType.TypeInference, typeExpr[1].NodeType);
            var inference = y.Parameter.Returns[1];
            Assert.AreEqual(ExpressionNodeType.TypeInference, inference.NodeType);
            var castInference = (PartialTypeInferenceExpression)inference;
            Assert.AreEqual(1, castInference.InferenceName.Count());
            Assert.AreEqual("T", castInference.InferenceName.First().Value);
            Assert.AreEqual(1, castInference.InferenceExpression.Count());
            Assert.AreEqual(ExpressionNodeType.Identifier, castInference.InferenceExpression.First().NodeType);
            Assert.AreEqual("any", castInference.InferenceExpression.Cast<IdentifierExpression>().First().Identifier.Value);
        }
    }
}
