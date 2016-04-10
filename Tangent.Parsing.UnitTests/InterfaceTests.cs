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
        public void InterfaceEmptyPath()
        {
            var test = Tokenize.ProgramFile("interface {}", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, takes);
        }

        [TestMethod]
        public void InterfaceMissingClose()
        {
            var test = Tokenize.ProgramFile("interface {", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsFalse(result.Success);
        }


        [TestMethod]
        public void InterfaceMissingOpen()
        {
            var test = Tokenize.ProgramFile("interface }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void InterfaceCannotHaveImplementations()
        {
            var test = Tokenize.ProgramFile("interface { foo => int { 42; } }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void InterfaceBasicSig()
        {
            var test = Tokenize.ProgramFile("interface { foo => int; }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(7, takes);
        }

        [TestMethod]
        public void InterfaceBasicThisSig()
        {
            var test = Tokenize.ProgramFile("interface { (this) foo => int; }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10, takes);
        }

        [TestMethod]
        public void InterfaceConversionSig()
        {
            var test = Tokenize.ProgramFile("interface { (this) => int; }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(9, takes);
        }

        [TestMethod]
        public void InterfaceOpSig()
        {
            var test = Tokenize.ProgramFile("interface { (this) + (this) => int; }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(13, takes);
        }

        [TestMethod]
        public void InterfaceArgSig()
        {
            var test = Tokenize.ProgramFile("interface { (this) (x:T:int) => int; }", "test.tan");
            int takes;
            var result = Grammar.InterfaceDecl.Parse(test, out takes);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(16, takes);
        }
    }
}
