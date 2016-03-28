using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests
{
    // ( name : type )
    //
    // name ::= (id|param-param)+
    // type ::= (id|lazy|param-inference)+
    [TestClass]
    public class ParamDeclTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var tokens = Tokenize.ProgramFile("(x:int)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, takes);
        }

        [TestMethod]
        public void MissingNameFails()
        {
            var tokens = Tokenize.ProgramFile("(:int)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingTypeFails()
        {
            var tokens = Tokenize.ProgramFile("(x:)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ComplexPath()
        {
            var tokens = Tokenize.ProgramFile("(fn(int):~>list(t))", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(12, takes);
        }

        [TestMethod]
        public void ComplexPathWithConstraints()
        {
            var tokens = Tokenize.ProgramFile("(fn(int):~>list(t:eq))", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(14, takes);
        }

        [TestMethod]
        public void MissingOpenFails()
        {
            var tokens = Tokenize.ProgramFile("x:int)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingCloseFails()
        {
            var tokens = Tokenize.ProgramFile("(x:int", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ConstrainedGenericPath()
        {
            var tokens = Tokenize.ProgramFile("(x:T:int)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(7, takes);
        }

        [TestMethod]
        public void ConstrainedGenericPhrasePath()
        {
            var tokens = Tokenize.ProgramFile("(some value:some type:some interface)", "test.tan");
            int takes;
            var result = Grammar.ParamDecl.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10, takes);
        }
    }
}
