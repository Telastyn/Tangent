using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Tokenization.UnitTests
{

    [TestClass]
    public class TokenizeTests
    {

        [TestMethod]
        public void EndOfFileGraceful()
        {
            var test = "";
            Assert.AreEqual(0, Tokenize.Skip(test, 0));
        }

        [TestMethod]
        public void PastEndOfFileGraceful()
        {
            var test = "foo";
            Assert.AreEqual(3, Tokenize.Skip(test, 42));
        }

        [TestMethod]
        public void SkipCommentsWorks()
        {
            var test = @"foo // blah blah
bar";
            Assert.AreEqual(17, Tokenize.Skip(test, 4));
        }

        [TestMethod]
        public void SkipCommentsEofGraceful()
        {
            var test = @"foo // blah blah";
            Assert.AreEqual(test.Length, Tokenize.Skip(test, 4));
        }

        [TestMethod]
        public void SkipSpacesWorks()
        {
            var test = "   \r\n";
            Assert.AreEqual(test.Length, Tokenize.Skip(test, 0));
        }

        [TestMethod]
        public void SkipSpacesStopsAtNonSpace1()
        {
            var test = " \t\r\nfoo";
            Assert.AreEqual(4, Tokenize.Skip(test, 0));
        }

        [TestMethod]
        public void SkipSpacesStopsAtNonSpace2()
        {
            var test = " \t\r\n-";
            Assert.AreEqual(4, Tokenize.Skip(test, 0));
        }

        [TestMethod]
        public void SkipSpacesNoopWhenOnWord()
        {
            var test = "foo";
            Assert.AreEqual(0, Tokenize.Skip(test, 0));
        }

        [TestMethod]
        public void ReductionMatches()
        {
            var test = "=>";
            var result = Tokenize.ProgramFile(test, "test.tan");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(TokenIdentifier.ReductionDeclSeparator, result.First().Identifier);
        }

        [TestMethod]
        public void TypeDeclarationSeparatorMatches()
        {
            var test = ":>";
            var result = Tokenize.ProgramFile(test, "test.tan");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(TokenIdentifier.TypeDeclSeparator, result.First().Identifier);
        }

        [TestMethod]
        public void IdentifierHappyPath1()
        {
            var test = "foo";
            var result = Tokenize.Identifier("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Identifier, result.Identifier);
            Assert.AreEqual("foo", result.Value);
        }

        [TestMethod]
        public void IdentifierHappyPath2()
        {
            var test = "foo-";
            var result = Tokenize.Identifier("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Identifier, result.Identifier);
            Assert.AreEqual("foo", result.Value);
        }

        [TestMethod]
        public void IdentifierHappyPath3()
        {
            var test = "foo ";
            var result = Tokenize.Identifier("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Identifier, result.Identifier);
            Assert.AreEqual("foo", result.Value);
        }

        [TestMethod]
        public void IdentifierHappyPath4()
        {
            var test = "fooあ";
            var result = Tokenize.Identifier("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Identifier, result.Identifier);
            Assert.AreEqual("foo", result.Value);
        }

        [TestMethod]
        public void SymbolHappyPath1()
        {
            var test = "あ";
            var result = Tokenize.Symbol("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Symbol, result.Identifier);
            Assert.AreEqual("あ", result.Value);
        }

        [TestMethod]
        public void SymbolHappyPath2()
        {
            var test = "+";
            var result = Tokenize.Symbol("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Symbol, result.Identifier);
            Assert.AreEqual("+", result.Value);
        }

        [TestMethod]
        public void SymbolHappyPath3()
        {
            var test = "++";
            var result = Tokenize.Symbol("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Symbol, result.Identifier);
            Assert.AreEqual("+", result.Value);
        }

        [TestMethod]
        public void SymbolHappyPath4()
        {
            var test = "+ ";
            var result = Tokenize.Symbol("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Symbol, result.Identifier);
            Assert.AreEqual("+", result.Value);
        }

        [TestMethod]
        public void SymbolHappyPath5()
        {
            var test = "+foo";
            var result = Tokenize.Symbol("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.Symbol, result.Identifier);
            Assert.AreEqual("+", result.Value);
        }

        [TestMethod]
        public void StringConstantHappyPath()
        {
            var test = "\"foo\"";
            var result = Tokenize.String("test.tan", test, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(TokenIdentifier.StringConstant, result.Identifier);
            Assert.AreEqual(1, result.SourceInfo.StartPosition.Column);
            Assert.AreEqual(6, result.SourceInfo.EndPosition.Column);
            Assert.AreEqual("\"foo\"", result.Value);
        }

        [TestMethod]
        public void MalformedStringIsNull()
        {
            var test = "\"foo";
            var result = Tokenize.String("test.tan", test, 0);

            Assert.IsNull(result);
        }
    }
}
