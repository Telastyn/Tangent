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
    }
}
