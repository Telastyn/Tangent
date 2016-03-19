using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Cli.TestSuite
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TestExpectationsViaGrammar
    {
        [TestMethod]
        public void Entrypoint()
        {
            var result = Test.DebugProgramFileViaGrammar("Entrypoint.tan");
            // No boom? Good.
        }

        [TestMethod]
        public void Moo()
        {
            var result = Test.DebugProgramFileViaGrammar("moo.tan");
            Assert.AreEqual("moo.", result.Trim());
        }

        [TestMethod]
        public void IntSandbox()
        {
            var result = Test.DebugProgramFileViaGrammar("intSandbox.tan");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "3", "True", "False" }));
        }

        [TestMethod]
        public void IfEqualsFalseCase()
        {
            var result = Test.DebugProgramFileViaGrammar("IfEqualsFalseCase.tan");
            Assert.AreEqual("zzz.", result.Trim());
        }


        [TestMethod]
        public void IfEqualsTrueCase()
        {
            var result = Test.DebugProgramFileViaGrammar("IfEqualsTrueCase.tan");
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void IfTrueCase()
        {
            var result = Test.DebugProgramFileViaGrammar("IfTrue.tan");
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void IfFalseCase()
        {
            var result = Test.DebugProgramFileViaGrammar("IfFalse.tan");
            Assert.AreEqual("zzz.", result.Trim());
        }

        [TestMethod]
        public void BasicCtorParams()
        {
            var result = Test.DebugProgramFileViaGrammar("BasicCtorParams.tan");
            Assert.AreEqual("42", result.Trim());
        }

        [TestMethod]
        public void AliasingTypesWorks()
        {
            var result = Test.DebugProgramFileViaGrammar("alias.tan");
            Assert.AreEqual("42", result.Trim());
        }

        [TestMethod]
        public void BasicAlgebraicDataTypes()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("adt.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1", "2", "3" }));
        }

        [TestMethod]
        public void BasicPartialSpecialization()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("PartialSpecialization.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in generic", "in foo generic", "in foo int" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicRuntimeInference()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("RuntimeInference.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in inference", "in int." }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicCompiletimeInference()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("SimpleInference.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in inference", "in int." }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicLambda()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("BasicLambda.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "2" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaResolutionByBody()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("LambdaResolutionByBody.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "44" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaReturn()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("LambdaReturn.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "16" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaResolutionByBodyReturn()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("LambdaResolutionByBodyReturn.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "with int", "42", "with void", "42" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void InterfaceNotEquals()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFileViaGrammar("interface-not-equals.tan", out compileDuration, out programDuration);
            Assert.AreEqual("true", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }
    }
}
