using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.UnitTests
{

    [TestClass]
    public class PartialStatementParseTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var test = Tokenize.ProgramFile("a b c;", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Result.Count());
            Assert.IsTrue(result.Result.All(e => e.Type == Partial.ElementType.Identifier));
            Assert.IsTrue(new List<Identifier>() { "a", "b", "c" }.SequenceEqual(result.Result.Select(e=>((IdentifierElement)e).Identifier)));
        }

        [TestMethod]
        public void BlocksWork()
        {
            var test = Tokenize.ProgramFile("a {b;} c;", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Result.Count());
            var bits = result.Result.ToList();
            Assert.AreEqual(ElementType.Identifier, bits[0].Type);
            Assert.AreEqual(ElementType.Block, bits[1].Type);
            Assert.AreEqual(ElementType.Identifier, bits[2].Type);
            var block = bits[1] as BlockElement;
            Assert.AreEqual(1, block.Block.Statements.Count());
            Assert.AreEqual(1, block.Block.Statements.First().FlatTokens.Count());
            Assert.AreEqual(ElementType.Identifier, block.Block.Statements.First().FlatTokens.First().Type);
            var b = block.Block.Statements.First().FlatTokens.First() as IdentifierElement;
            Assert.AreEqual("b", b.Identifier.Value);
        }

        [TestMethod]
        public void SymbolsParse()
        {
            var test = Tokenize.ProgramFile("a + c;", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void StopAtSemi()
        {
            var test = Tokenize.ProgramFile("a b c; +", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void MissingSemiErrors()
        {
            var test = Tokenize.ProgramFile("a b c", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void LambdasPullLeadingIdentifier()
        {
            var test = Tokenize.ProgramFile("x=>{};", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Result.Count());
            Assert.AreEqual(ElementType.Lambda, result.Result.First().Type);
            var l = (LambdaElement)result.Result.First();
            Assert.AreEqual(1, l.Takes.Count);
            Assert.AreEqual("x", l.Takes.First().ParameterDeclaration.Takes.First().Identifier);
            Assert.AreEqual(null, l.Takes.First().ParameterDeclaration.Returns);
        }

        [TestMethod]
        public void LambdasPullManyVardecls()
        {
            var test = Tokenize.ProgramFile("(x)(x y)(x y z)=>{};", "test.tan");

            var result = Parse.PartialStatement(test.ToList());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Result.Count());
            Assert.AreEqual(ElementType.Lambda, result.Result.First().Type);
            var l = (LambdaElement)result.Result.First();
            Assert.AreEqual(3, l.Takes.Count);
            Assert.AreEqual("x", l.Takes.First().ParameterDeclaration.Takes.First().Identifier);
            Assert.AreEqual(null, l.Takes.First().ParameterDeclaration.Returns);
            Assert.AreEqual("x", l.Takes.First().ParameterDeclaration.Takes.First().Identifier);
            Assert.AreEqual("y", l.Takes.First().ParameterDeclaration.Takes.Skip(1).First().Identifier);
            Assert.AreEqual(null, l.Takes.First().ParameterDeclaration.Returns);
            Assert.AreEqual("x", l.Takes.First().ParameterDeclaration.Takes.First().Identifier);
            Assert.AreEqual("y", l.Takes.First().ParameterDeclaration.Takes.Skip(1).First().Identifier);
            Assert.AreEqual("z", l.Takes.First().ParameterDeclaration.Takes.Skip(2).First().Identifier);
            Assert.AreEqual(null, l.Takes.First().ParameterDeclaration.Returns);
        }
    }
}
