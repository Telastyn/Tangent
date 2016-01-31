using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class InterfaceTests
    {
        [TestMethod]
        public void NoInterfaceMeansNull()
        {
            var bits = Tokenize.ProgramFile("foo", "test").ToList();
            Assert.IsNull(Parse.TryInterface(bits, Enumerable.Empty<PartialParameterDeclaration>()));
        }

        [TestMethod]
        public void EmptyInterfaceIsFine()
        {
            var bits = Tokenize.ProgramFile("interface{}", "test").ToList();
            var result = Parse.TryInterface(bits, Enumerable.Empty<PartialParameterDeclaration>());
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BadPhraseErrors()
        {
            var bits = Tokenize.ProgramFile("interface{ => void; }", "test").ToList();
            var result = Parse.TryInterface(bits, Enumerable.Empty<PartialParameterDeclaration>());
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FullFunctionErrors()
        {
            var bits = Tokenize.ProgramFile("interface{ x => void{}; }", "test").ToList();
            var result = Parse.TryInterface(bits, Enumerable.Empty<PartialParameterDeclaration>());
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void HappyPath()
        {
            var bits = Tokenize.ProgramFile(@"
interface{ 
  (this) => string; 
  execute (this) => void;
}", "test").ToList();
            var result = Parse.TryInterface(bits, Enumerable.Empty<PartialParameterDeclaration>());
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, ((PartialInterface)result.Result).Functions.Count);
        }
    }
}
