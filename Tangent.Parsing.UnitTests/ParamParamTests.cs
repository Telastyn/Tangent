using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Intermediate;

namespace Tangent.Parsing.UnitTests
{
    // (type-expr)
    // ((id|lazy)+)
    [TestClass]
    public class ParamParamTests
    {
        [TestMethod]
        public void RequiresSomething()
        {
            var tokens = Tokenize.ProgramFile("()", "test.tan");
            int takes;
            var result = Grammar.ParamParam.Parse(tokens, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void SimpleCase()
        {
            var tokens = Tokenize.ProgramFile("(foo)", "test.tan");
            int takes;
            var result = Grammar.ParamParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, takes);
            Assert.IsFalse(result.Result.IsIdentifier);
        }


        [TestMethod]
        public void ManyCase()
        {
            var tokens = Tokenize.ProgramFile("(foo bar baz)", "test.tan");
            int takes;
            var result = Grammar.ParamParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, takes);
            Assert.IsFalse(result.Result.IsIdentifier);
        }

        [TestMethod]
        public void ManyCaseWithLazy()
        {
            var tokens = Tokenize.ProgramFile("(foo ~> baz)", "test.tan");
            int takes;
            var result = Grammar.ParamParam.Parse(tokens, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, takes);
            Assert.IsFalse(result.Result.IsIdentifier);
        }
    }
}
