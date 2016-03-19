using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BlockTests
    {
        [TestMethod]
        public void EmptyBlockWorks()
        {
            var test = Tokenize.ProgramFile("{}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void SemiColonOptional()
        {
            var test = Tokenize.ProgramFile("{a;b}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }


        [TestMethod]
        public void SemiColonUsed()
        {
            var test = Tokenize.ProgramFile("{a;b;}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }
    }
}
