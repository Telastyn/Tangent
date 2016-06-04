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

        [TestMethod]
        public void LocalBasicCase()
        {
            var test = Tokenize.ProgramFile("{:x:int := 42;}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Result.Locals.Count());
            var local = result.Result.Locals.First();
            Assert.AreEqual(1, local.Initializer.FlatTokens.Count());
        }

        [TestMethod]
        public void LocalsNeedNotBeFirst()
        {
            var test = Tokenize.ProgramFile("{a; :x:int := 42;}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Result.Locals.Count());
            var local = result.Result.Locals.First();
            Assert.AreEqual(1, local.Initializer.FlatTokens.Count());
            Assert.AreEqual(2, result.Result.Statements.Count());
        }

        [TestMethod]
        public void MultipleLocals()
        {
            var test = Tokenize.ProgramFile("{:x:int := 42; :y:string := \"foo\"; :z:bool := true}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Result.Locals.Count());
        }

        [TestMethod]
        public void LocalPhrase()
        {
            var test = Tokenize.ProgramFile("{:some x:some int := 42 + 42;}", "test.tan");
            int takes;
            var result = Grammar.BlockDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Result.Locals.Count());
            var local = result.Result.Locals.First();
            Assert.AreEqual(3, local.Initializer.FlatTokens.Count());
        }
    }
}
