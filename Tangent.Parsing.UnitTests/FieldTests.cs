using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    public class FieldTests
    {
        [TestMethod]
        public void BasicInitializationCase()
        {
            var input = @"foo :> new Foo(x: int) {
                (this).test : int := 42 + x;
                (this) => string { this.test }
            }";

            var tokens = Tokenize.ProgramFile(input, "test.tan");
            int taken;
            var result = Grammar.TypeDecl.Parse(tokens, out taken);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(tokens.Count(), taken);
        }
    }
}
