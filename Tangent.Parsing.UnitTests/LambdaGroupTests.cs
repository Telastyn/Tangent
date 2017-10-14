using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LambdaGroupTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var test = Tokenize.ProgramFile(":< x { int => print x; }", "test.tan");
            int takes;
            var result = Grammar.LambdaGroupExpr.Parse(test, out takes);

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void NoImplementationsFails()
        {
            var test = Tokenize.ProgramFile(":< x { }", "test.tan");
            int takes;
            var result = Grammar.LambdaGroupExpr.Parse(test, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void MissingTrailingSemicolonFails()
        {
            var test = Tokenize.ProgramFile(":< x { int => print x }", "test.tan");
            int takes;
            var result = Grammar.LambdaGroupExpr.Parse(test, out takes);

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void PhraseParameterWorks()
        {
            var test = Tokenize.ProgramFile(":< some var { int => print some var; }", "test.tan");
            int takes;
            var result = Grammar.LambdaGroupExpr.Parse(test, out takes);

            Assert.IsTrue(result.Success);
        }
        

        [TestMethod]
        public void MultipleLambdas()
        {
            var test = Tokenize.ProgramFile(":< x { int => print x; some type => print \"unknown\"; }", "test.tan");
            int takes;
            var result = Grammar.LambdaGroupExpr.Parse(test, out takes);

            Assert.IsTrue(result.Success);
        }
    }
}
