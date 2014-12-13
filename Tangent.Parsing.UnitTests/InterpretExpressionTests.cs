using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing.UnitTests {
    
    [TestClass]
    public class InterpretExpressionTests {

        [TestMethod]
        public void SimpleParameterHappyPath() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", TangentType.Void) },
                Enumerable.Empty<TypeResolvedReductionDeclaration>());

            var statement = new List<Identifier>() { "foo" };

            var result = InterpretExpression.ForStatement(statement, scope);

            Assert.IsNotNull(result);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.NodeType);

            var parameter = result as ParameterAccessExpression;
            Assert.AreEqual(scope.Parameters.First(), parameter.Parameter);
        }

        [TestMethod]
        public void SimpleFunctionHappyPath() {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[]{ new TypeResolvedReductionDeclaration(new PhrasePart("foo"), new TypeResolvedFunction(TangentType.Void, new PartialBlock(Enumerable.Empty<PartialStatement>())))});

            var statement = new List<Identifier>() { "foo" };

            var result = InterpretExpression.ForStatement(statement, scope);

            Assert.IsNotNull(result);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.NodeType);

            var fn = result as FunctionInvocationExpression;
            Assert.AreEqual(scope.Functions.First().Returns, fn.Bindings.FunctionDefinition.Returns);
        }
    }
}
