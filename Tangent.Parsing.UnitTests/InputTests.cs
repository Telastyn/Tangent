using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Tokenization;

namespace Tangent.Parsing.UnitTests
{

    [TestClass]
    public class InputTests
    {

        [TestMethod]
        public void BasicParameterResolution()
        {
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
        public void ParameterPhraseResolution()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration(new Identifier[] { "foo", "bar" }, TangentType.Void) },
                Enumerable.Empty<ReductionDeclaration>());

            var tokens = Tokenize.ProgramFile("foo bar").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)result.First()).Parameter);
        }

        [TestMethod]
        public void BasicFunctionResolution()
        {
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
        public void FunctionPhraseResolution()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new PhrasePart[] { new PhrasePart("foo"), new PhrasePart("bar") }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("foo bar").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(scope.Functions.First(), ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void BasicFunctionConsumption()
        {
            // foo (x: t) => void;
            // (bar: t) => * {
            //    foo bar;
            // }
            var t = new EnumType(Enumerable.Empty<Identifier>());
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
        public void ReverseFunctionConsumption()
        {
            // (x: t) foo => void;
            // (bar: t) => * {
            //    bar foo;
            // }
            var t = new EnumType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new ParameterDeclaration("x", t)), new PhrasePart(new Identifier("foo")) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("bar foo").Select(token => new Identifier(token.Value));

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
        public void InfixFunctionConsumption()
        {
            // (x: t) foo (y: t) => void;
            // (bar: t) => * {
            //    bar foo bar;
            // }
            var t = new EnumType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new ParameterDeclaration("x", t)), new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("y", t)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) });

            var tokens = Tokenize.ProgramFile("bar foo bar").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());
            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(2, invoke.Bindings.Parameters.Count());
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Parameters.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Parameters.First()).Parameter);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Parameters.Skip(1).First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Parameters.Skip(1).First()).Parameter);
        }

        [TestMethod]
        public void MismatchReturnsNoResults()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ReductionDeclaration>());

            var tokens = Tokenize.ProgramFile("foo").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParametersWinScope()
        {
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
        public void ReturnTypesMatter()
        {
            var t = new EnumType(Enumerable.Empty<Identifier>());
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

        [TestMethod]
        public void LazyWorksWithBindings()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { 
                    new ReductionDeclaration(new PhrasePart[] { new PhrasePart("f"), new ParameterDeclaration("x", TangentType.Void.Lazy) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) ,
                    new ReductionDeclaration(new PhrasePart[]{ new PhrasePart("g")}, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())))
                });

            var tokens = Tokenize.ProgramFile("f g").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(ExpressionNodeType.HalfBoundExpression, ((FunctionInvocationExpression)result.First()).Bindings.Parameters.First().NodeType);
        }

        [TestMethod]
        public void LazyWorksWithParams()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("g", TangentType.Void) },
                new[] { 
                    new ReductionDeclaration(new PhrasePart[] { new PhrasePart("f"), new ParameterDeclaration("x", TangentType.Void.Lazy) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())))
                });

            var tokens = Tokenize.ProgramFile("f g").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(ExpressionNodeType.HalfBoundExpression, ((FunctionInvocationExpression)result.First()).Bindings.Parameters.First().NodeType);
            var hbe = ((FunctionInvocationExpression)result.First()).Bindings.Parameters.First() as HalfBoundExpression;
            dynamic decl = new PrivateObject(hbe).GetField("Declaration");
            Assert.IsTrue(scope.Parameters.First() == decl);
        }

        [TestMethod]
        public void BuiltinBasicPath()
        {
            var scope = new Scope(
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                BuiltinFunctions.All);

            var result = new Input(new Expression[] { new IdentifierExpression("print"), new ConstantExpression<string>(TangentType.String, "foo") }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(BuiltinFunctions.PrintString, ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
            Assert.AreEqual(ExpressionNodeType.Constant, ((FunctionInvocationExpression)result.First()).Bindings.Parameters.First().NodeType);
            Assert.AreEqual("foo", ((ConstantExpression<string>)((FunctionInvocationExpression)result.First()).Bindings.Parameters.First()).TypedValue);
        }

        [TestMethod]
        public void EnumValueBasicPath()
        {
            Identifier bar = "bar";
            var foo = new EnumType(new[] { bar });
            var foodecl = new TypeDeclaration("foo", foo);
            var scope = new Scope(
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo.SingleValueTypeFor(bar))), new Function(TangentType.Void, null)) });

            var result = new Input(new Expression[] { new IdentifierExpression("bar") }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void SpecializationWins()
        {
            Identifier bar = "bar";
            var foo = new EnumType(new[] { bar });
            var foodecl = new TypeDeclaration("foo", foo);

            var special = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo.SingleValueTypeFor( bar))), new Function(TangentType.Void, null));
            var generic = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo)), new Function(TangentType.Void, null));
            var scope = new Scope(
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { special, generic });

            var result = new Input(new Expression[] { new IdentifierExpression("bar") }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(special, ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void FallbackToEnumWorks()
        {
            Identifier bar = "bar";
            var foo = new EnumType(new[] { bar });
            var foodecl = new TypeDeclaration("foo", foo);

            var generic = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo)), new Function(TangentType.Void, null));
            var scope = new Scope(
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { generic });

            var result = new Input(new Expression[] { new IdentifierExpression("bar") }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(generic, ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void BasicIfTest()
        {
            Identifier t = "true";
            Identifier f = "false";
            var boolean = new EnumType(new[] { t, f });
            var booleanDecl = new TypeDeclaration("bool", boolean);
            var ifFalse = new ReductionDeclaration(new PhrasePart[]{
                new PhrasePart("if"),
                new PhrasePart(new ParameterDeclaration("condition", boolean)),
                new PhrasePart(new ParameterDeclaration("positive", TangentType.Void.Lazy))},

                new Function(TangentType.Void, null));

            var ifTrue = new ReductionDeclaration(new PhrasePart[]{
                new PhrasePart("if"),
                new PhrasePart(new ParameterDeclaration("condition", boolean.SingleValueTypeFor(t))),
                new PhrasePart(new ParameterDeclaration("positive", TangentType.Void.Lazy))},

                new Function(TangentType.Void, null));

            var scope = new Scope(
                new[] { booleanDecl },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { ifTrue, ifFalse });

            var result = new Input(new Expression[] { new IdentifierExpression("if"), new IdentifierExpression("true"), new FunctionBindingExpression(new ReductionDeclaration(Enumerable.Empty<PhrasePart>(), new Function(TangentType.Void, null)), new Expression[] { }) }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
