using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TypeDeclParamTests
    {
        //(id+(:id+)?)
        [TestMethod]
        public void EmptyParensFail()
        {
            var tokens = Tokenize.ProgramFile("()", "test.tan");
            int takes;
            var result = Grammar.TypeDeclParam.Parse(tokens, out takes);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void SingleIdSucceeds()
        {
            var tokens = Tokenize.ProgramFile("(x)", "test.tan");
            int takes;
            var result = Grammar.TypeDeclParam.Parse(tokens, out takes);
            
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, takes);
            Assert.AreEqual(1, result.Result.Takes.Count);
            Assert.AreEqual("x", result.Result.Takes.First().Identifier.Identifier);
            Assert.AreEqual(1, result.Result.Returns.Count);
            Assert.IsTrue(result.Result.Returns.First() is IdentifierExpression);
            Assert.AreEqual("any", (result.Result.Returns.First() as IdentifierExpression).Identifier);
        }

        [TestMethod]
        public void MultipleIdSucceeds()
        {
            var tokens = Tokenize.ProgramFile("(x +y)", "test.tan");
            int takes;
            var result = Grammar.TypeDeclParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, takes);
            Assert.AreEqual(3, result.Result.Takes.Count);
            Assert.AreEqual("x", result.Result.Takes.First().Identifier.Identifier);
            Assert.AreEqual("+", result.Result.Takes.Skip(1).First().Identifier.Identifier);
            Assert.AreEqual("y", result.Result.Takes.Skip(2).First().Identifier.Identifier);
            Assert.AreEqual(1, result.Result.Returns.Count);
            Assert.IsTrue(result.Result.Returns.First() is IdentifierExpression);
            Assert.AreEqual("any", (result.Result.Returns.First() as IdentifierExpression).Identifier);
        }

        [TestMethod]
        public void GenericConstraintRecognized()
        {
            var tokens = Tokenize.ProgramFile("(x :: int)", "test.tan");
            int takes;
            var result = Grammar.TypeDeclParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(6, takes);
            Assert.AreEqual(1, result.Result.Takes.Count);
            Assert.AreEqual("x", result.Result.Takes.First().Identifier.Identifier);
            Assert.AreEqual(1, result.Result.Returns.Count);
            Assert.IsTrue(result.Result.Returns.First() is IdentifierExpression);
            Assert.AreEqual("int", (result.Result.Returns.First() as IdentifierExpression).Identifier);
        }


        [TestMethod]
        public void LongGenericConstraintRecognized()
        {
            var tokens = Tokenize.ProgramFile("(x :: int+y)", "test.tan");
            int takes;
            var result = Grammar.TypeDeclParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(8, takes);
            Assert.AreEqual(1, result.Result.Takes.Count);
            Assert.AreEqual("x", result.Result.Takes.First().Identifier.Identifier);
            Assert.AreEqual(3, result.Result.Returns.Count);
            Assert.IsTrue(result.Result.Returns.All(r=>r is IdentifierExpression));
            Assert.AreEqual("int", (result.Result.Returns.First() as IdentifierExpression).Identifier);
            Assert.AreEqual("+", (result.Result.Returns.Skip(1).First() as IdentifierExpression).Identifier);
            Assert.AreEqual("y", (result.Result.Returns.Skip(2).First() as IdentifierExpression).Identifier);
        }
    }
}
