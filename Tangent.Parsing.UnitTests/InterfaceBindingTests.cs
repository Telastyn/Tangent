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

        [TestMethod]
        public void SimpleStandaloneBinding()
        {
            var test = Tokenize.ProgramFile("bar :< ifoo {}", "test.tan");
            int takes;
            var result = Grammar.StandaloneInterfaceBinding.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }

        [TestMethod]
        public void PhraseStandaloneBinding()
        {
            var test = Tokenize.ProgramFile("bar :< i foo {}", "test.tan");
            int takes;
            var result = Grammar.StandaloneInterfaceBinding.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }

        [TestMethod]
        public void ParameterizedPhraseStandaloneBinding()
        {
            var test = Tokenize.ProgramFile("bar (T) :< i foo<T> {}", "test.tan");
            int takes;
            var result = Grammar.StandaloneInterfaceBinding.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
        }

        [TestMethod]
        public void MultipleStandaloneBinding()
        {
            var test = Tokenize.ProgramFile("bar :< ifoo :< ibar{}", "test.tan");
            int takes;
            var result = Grammar.StandaloneInterfaceBinding.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
            Assert.AreEqual(2, result.Result.InterfaceReferences.Count);
        }

        [TestMethod]
        public void SimpleStandaloneBindingWithFunction()
        {
            var test = Tokenize.ProgramFile("bar :< ifoo { barify => int { 42 }}", "test.tan");
            int takes;
            var result = Grammar.StandaloneInterfaceBinding.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(test.Count(), takes);
            Assert.AreEqual(1, result.Result.Functions.Count);
        }
    }
}
