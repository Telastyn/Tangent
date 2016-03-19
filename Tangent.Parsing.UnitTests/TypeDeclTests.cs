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
    public class TypeDeclTests
    {
        [TestMethod]
        public void BasicEnum()
        {
            var test = Tokenize.ProgramFile("foo :> enum { a, b, c }", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BasicAlias()
        {
            var test = Tokenize.ProgramFile("foo :> bar;", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void AliasSum()
        {
            var test = Tokenize.ProgramFile("foo :> bar | baz | int;", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void AliasSumWithClass()
        {
            var test = Tokenize.ProgramFile("foo :> bar | baz | int {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BasicClass()
        {
            var test = Tokenize.ProgramFile("foo :> bar {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void BasicClass2()
        {
            var test = Tokenize.ProgramFile("foo :> (x: int), (y: int) {}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Adt1()
        {
            var test = Tokenize.ProgramFile(@"
int list :> int | (a: int),(b: int list) {
}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }
        
        [TestMethod]
        public void Adt2()
        {
            var test = Tokenize.ProgramFile(@"
int list :> int | (a: int),(b: int list) {
  (this).head => int { a }
  (this).tail => int list { b }
  print (this) => void {
    print this.head;
	print this.tail;
  }
}", "test.tan");
            int takes;
            var result = Grammar.TypeDecl.Parse(test, out takes);

            Assert.AreEqual(test.Count(), takes);
            Assert.IsTrue(result.Success);
        }
    }
}
