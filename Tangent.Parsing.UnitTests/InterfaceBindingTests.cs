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
    public class InterfaceBindingTests
    {
        [TestMethod]
        public void SimpleInlineBinding()
        {
            var test = Tokenize.ProgramFile("foo :> bar :< ifoo {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }

        [TestMethod]
        public void CtorPhraseInlineBinding() {
            var test = Tokenize.ProgramFile("foo :> bar baz (x: int) :< ifoo {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }

        [TestMethod]
        public void InterfaceMultiIdentiferInlineBinding() {
            var test = Tokenize.ProgramFile("foo :> bar :< foo interface {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }


        [TestMethod]
        public void SumTypeInlineBinding() {
            var test = Tokenize.ProgramFile("foo :> bar | baz :< foo interface {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }
    }
}
