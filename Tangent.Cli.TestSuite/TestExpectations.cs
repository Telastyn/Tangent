using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Cli.TestSuite
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TestExpectations
    {
        [TestMethod]
        public void Entrypoint()
        {
            var result = Test.DebugProgramFile("Entrypoint.tan");
            // No boom? Good.
        }

        [TestMethod]
        public void Moo()
        {
            var result = Test.ProgramFile("moo.tan");
            Assert.AreEqual("moo.", result.Trim());
        }

        [TestMethod]
        public void IntSandbox()
        {
            var result = Test.ProgramFile("intSandbox.tan");
            var results = result.Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "3", "True", "False" }));
        }

        [TestMethod]
        public void IfEqualsFalseCase()
        {
            var result = Test.ProgramFile("IfEqualsFalseCase.tan");
            Assert.AreEqual("zzz.", result.Trim());
        }


        [TestMethod]
        public void IfEqualsTrueCase()
        {
            var result = Test.ProgramFile("IfEqualsTrueCase.tan");
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void IfTrueCase()
        {
            var result = Test.DebugProgramFile("IfTrue.tan");
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void IfFalseCase()
        {
            var result = Test.ProgramFile("IfFalse.tan");
            Assert.AreEqual("zzz.", result.Trim());
        }

        [TestMethod]
        public void BasicCtorParams()
        {
            var result = Test.DebugProgramFile("BasicCtorParams.tan");
            Assert.AreEqual("42", result.Trim());
        }

        [TestMethod]
        public void AliasingTypesWorks()
        {
            var result = Test.ProgramFile("alias.tan");
            Assert.AreEqual("42", result.Trim());
        }

        [TestMethod]
        public void BasicAlgebraicDataTypes()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("adt.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1", "2", "3" }));
        }

        [TestMethod]
        public void BasicPartialSpecialization()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.ProgramFile("PartialSpecialization.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in generic", "in foo generic", "in foo int" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicRuntimeInference()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.ProgramFile("RuntimeInference.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in inference", "in int." }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicCompiletimeInference()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.ProgramFile("SimpleInference.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in inference", "in int." }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicLambda()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("BasicLambda.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "2" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaResolutionByBody()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("LambdaResolutionByBody.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "44" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaReturn()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("LambdaReturn.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "16" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void LambdaResolutionByBodyReturn()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("LambdaResolutionByBodyReturn.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "with int", "42", "with void", "42" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void InterfaceNotEquals(){
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("interface-not-equals.tan", out compileDuration, out programDuration);
            Assert.AreEqual("true", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }
    }
}
