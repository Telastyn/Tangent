﻿using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate.Interop;
using System.Collections.Generic;

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
            var result = Test.DebugProgramFile("moo.tan");
            Assert.AreEqual("moo.", result.Trim());
        }

        [TestMethod]
        public void IntSandbox()
        {
            var result = Test.ProgramFile("intSandbox.tan");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
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
        public void IfTrueBraceCase()
        {
            var result = Test.DebugProgramFile("IfTrue-Braces.tan");
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void IfFalseCase()
        {
            var result = Test.DebugProgramFile("IfFalse.tan");
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
        public void AliasingGenericTypesWorks()
        {
            var result = Test.DebugProgramFile("alias-generic.tan");
            Assert.AreEqual("in fn", result.Trim());
        }

        [TestMethod]
        public void BasicPartialSpecialization()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("PartialSpecialization.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "in generic", "in foo generic", "in foo int" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicRuntimeInference()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("RuntimeInference.tan", out compileDuration, out programDuration);
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
        public void ToStringFunctionInterface()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("ToStringFunctionInterface.tan", out compileDuration, out programDuration);
            Assert.AreEqual("moo...", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void ToStringFunctionInterfacePassing()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("ToStringFunctionInterfacePassing.tan", out compileDuration, out programDuration);
            Assert.AreEqual("moo...", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void ToStringConversionInterface()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("ToStringConversionInterface.tan", out compileDuration, out programDuration);
            Assert.AreEqual("moo...", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }


        [TestMethod]
        public void ToStringStandaloneBinding()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("ToStringStandaloneBinding.tan", out compileDuration, out programDuration);
            Assert.AreEqual("moo...", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void MultifileIfTrueRealCompiler()
        {
            var result = Test.ProgramFile(new[] { "IfTrue-NeedsLib.tan", @"lib\conditional-lib.tan" });
            Assert.AreEqual("w00t.", result.Trim());
        }

        [TestMethod]
        public void BasicLocals()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("BasicLocals.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "43" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicFields()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("BasicFields.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "47", "47", "5" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void InterfaceFields()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("InterfaceFields.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "?", "Bob", }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void InterfaceGetterOnly()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("InterfaceGetterOnly.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "?", "Bob", }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void BasicDelegateUse()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("BasicDelegateUse.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void AssignmentDelegateUse()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("AssignmentDelegateUse.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "46" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        public void SimpleImportReferenceProperty()
        {
            var result = Test.DebugProgramFile("SimpleImportReferenceProperty.tan", new[] { typeof(string).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "3", "6" }));
        }

        [TestMethod]
        public void NonBuiltinReferenceType()
        {
            var result = Test.DebugProgramFile("NonBuiltInReferenceType.tan", new[] { typeof(string).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { new object().ToString() }));
        }

        [TestMethod]
        public void IfTrueUsingBoolImport()
        {
            var result = Test.DebugProgramFile("IfTrue-using-bool-import.tan", new[] { typeof(bool).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "w00t." }));
        }

        [TestMethod]
        public void NestedIfElse()
        {
            var result = Test.DebugProgramFile(new[] { "NestedIfElse.tan", @"lib\conditional-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "equals" }));
        }

        [TestMethod]
        public void LocalClosures()
        {
            var result = Test.DebugProgramFile(new[] { "LocalClosures.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1", "2", "3" }));
        }

        [TestMethod]
        public void LoopTermination()
        {
            var result = Test.DebugProgramFile(new[] { "LoopTermination.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "0", "1", "2", "3", "4" }));
        }

        [TestMethod]
        public void BasicGenericClassInference()
        {
            var result = Test.DebugProgramFile(new[] { "BasicGenericClassInference.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "brown cow" }));
        }

        [TestMethod]
        public void SimpleDependentInference()
        {
            var result = Test.DebugProgramFile(new[] { "SimpleDependentInference.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "at checkpoint", "foo" }));
        }


        [TestMethod]
        public void ExplicitCtorTypeParam()
        {
            var result = Test.DebugProgramFile(new[] { "ExplicitCtorTypeParam.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "bar" }));
        }

        [TestMethod]
        public void ExplicitTypeParam()
        {
            var result = Test.DebugProgramFile(new[] { "ExplicitTypeParam.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "foo" }));
        }

        [TestMethod]
        public void ConsBenchmark()
        {
            var result = Test.ProgramFile(new[] { "cons-benchmark.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { }));
        }

        [TestMethod]
        public void GenericInteropConstructors()
        {
            var result = Test.DebugProgramFile(new[] { "GenericInteropConstructors.tan" }, new[] { typeof(List<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { }));
        }

        [TestMethod]
        public void GenericInteropInstanceOperations()
        {
            var result = Test.DebugProgramFile(new[] { "GenericInteropInstanceOperations.tan" }, new[] { typeof(List<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { "2" }));
        }

        [TestMethod]
        public void GenericPair()
        {
            var result = Test.DebugProgramFile(new[] { "GenericPair.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "True", "True" }));
        }

        [TestMethod]
        public void IfReadme()
        {
            var result = Test.DebugProgramFile(new[] { "IfReadme.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "x", "y" }));
        }

        [TestMethod]
        public void InterfaceReadme()
        {
            var result = Test.DebugProgramFile(new[] { "InterfaceReadme.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "True", "False" }));
        }

        [TestMethod]
        public void Ternary()
        {
            var result = Test.DebugProgramFile(new[] { "Ternary.tan", @"lib\conditional-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42" }));
        }

        [TestMethod]
        public void TernaryInferred()
        {
            var result = Test.DebugProgramFile(new[] { "TernaryInferred.tan", @"lib\conditional-lib.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42" }));
        }

        [TestMethod]
        public void InteropInterfaceUse()
        {
            var result = Test.DebugProgramFile(new[] { "InteropInterfaceUse.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1", "2", "3" }));
        }

        [TestMethod]
        public void InteropInterfaceUse2()
        {
            var result = Test.DebugProgramFile(new[] { "InteropInterfaceUse2.tan" }, new[] { typeof(IEnumerable<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1, 2, 3" }));
        }

        [TestMethod]
        public void InteropInterfaceUse3()
        {
            var result = Test.DebugProgramFile(new[] { "InteropInterfaceUse3.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "1", "2", "3" }));
        }

        [TestMethod]
        public void InferenceWithDelegate()
        {
            var result = Test.DebugProgramFile(new[] { "InferenceWithDelegate.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "foo", "42" }));
        }


        [TestMethod]
        public void ArrayInteropAccess()
        {
            var result = Test.DebugProgramFile(new[] { "ArrayInteropAccess.tan" }, new[] { typeof(List<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42" }));
        }

        [TestMethod]
        public void ArrayInteropAssignment()
        {
            var result = Test.DebugProgramFile(new[] { "ArrayInteropAssignment.tan" }, new[] { typeof(List<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "6" }));
        }

        [TestMethod]
        public void StringEscapes()
        {
            var result = Test.DebugProgramFile(new[] { "StringEscapes.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "\"", "\\" }));
        }

        [TestMethod]
        public void LambdaGroupSimple()
        {
            var result = Test.DebugProgramFile(new[] { "LambdaGroupSimple.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "nothing" }));
        }

        [TestMethod]
        public void LambdaGroupSimpleImpliedDefault()
        {
            var result = Test.DebugProgramFile(new[] { "LambdaGroupSimpleImpliedDefault.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "nothing" }));
        }

        [TestMethod]
        public void LambdaGroupGenericInterface()
        {
            var result = Test.DebugProgramFile(new[] { "LambdaGroupGenericInterface.tan" });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "nothing" }));
        }

        [TestMethod]
        public void ParserDabble()
        {
            var result = Test.DebugProgramFile(new[] { "ParserDabble.tan", @"lib\parser-lib.tan", @"lib\conditional-lib.tan", @"lib\string-lib.tan", @"lib\enumerable-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "true" }));
        }

        [TestMethod]
        [Ignore]
        public void BasicGlobalUse()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("BasicGlobalUse.tan", out compileDuration, out programDuration);
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new[] { "42", "4" }));
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }

        [TestMethod]
        [Ignore]
        public void InterfaceNotEquals()
        {
            TimeSpan compileDuration;
            TimeSpan programDuration;
            var result = Test.DebugProgramFile("interface-not-equals.tan", out compileDuration, out programDuration);
            Assert.AreEqual("true", result.Trim());
            Assert.IsTrue(compileDuration < TimeSpan.FromSeconds(1), "Compile time exceeds limit.");
        }
    }
}
