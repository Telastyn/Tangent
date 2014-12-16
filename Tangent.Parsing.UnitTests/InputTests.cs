using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests {

    [TestClass]
    public class InputTests {

        [TestMethod]
        public void BasicParameterResolution() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", TangentType.Void) },
                Enumerable.Empty<ReductionDeclaration>());

            var tokens = Tokenize.ProgramFile("foo").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)result.First()).Parameter);
        }

        [TestMethod]
        public void BasicFunctionResolution() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("foo").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(scope.Functions.First(), ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void BasicFunctionConsumption() {
            // foo (x: t) => void;
            // (bar: t) => * {
            //    foo bar;
            // }
            var t = new TangentType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("x", t)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("foo bar").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());
            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(1, invoke.Bindings.Parameters.Count());
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Parameters.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Parameters.First()).Parameter);
        }

        [TestMethod]
        public void MismatchReturnsNoResults() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ReductionDeclaration>());

            var tokens = Tokenize.ProgramFile("foo").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParametersWinScope() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", TangentType.Void) },
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("foo").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)result.First()).Parameter);
        }

        [TestMethod]
        public void ReturnTypesMatter() {
            var t = new TangentType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", t) },
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("foo").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(scope.Functions.First(), ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }
    }
}
