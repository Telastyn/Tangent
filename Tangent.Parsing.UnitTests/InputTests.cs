using System;
using System.Collections.Generic;
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
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", TangentType.Void) },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ReductionDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo", "test.tan").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)result.First()).Parameter);
        }


        [TestMethod]
        public void ParameterPhraseResolution()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration(new Identifier[] { "foo", "bar" }, TangentType.Void) },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ReductionDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo bar", "test.tan").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, result.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)result.First()).Parameter);
        }

        [TestMethod]
        public void BasicFunctionResolution()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo", "test.tan").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(scope.Functions.First(), ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void FunctionPhraseResolution()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new PhrasePart[] { new PhrasePart("foo"), new PhrasePart("bar") }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo bar", "test.tan").Select(t => new Identifier(t.Value));

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
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("x", t)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo bar", "test.tan").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());

            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(1, invoke.Bindings.Arguments.Count());
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Arguments.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Arguments.First()).Parameter);
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
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new ParameterDeclaration("x", t)), new PhrasePart(new Identifier("foo")) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("bar foo", "test.tan").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());
            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(1, invoke.Bindings.Arguments.Count());
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Arguments.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Arguments.First()).Parameter);
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
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new ParameterDeclaration("x", t)), new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("y", t)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("bar foo bar", "test.tan").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());
            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(2, invoke.Bindings.Arguments.Count());
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Arguments.First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Arguments.First()).Parameter);
            Assert.AreEqual(ExpressionNodeType.ParameterAccess, invoke.Bindings.Arguments.Skip(1).First().NodeType);
            Assert.AreEqual(scope.Parameters.First(), ((ParameterAccessExpression)invoke.Bindings.Arguments.Skip(1).First()).Parameter);
        }

        [TestMethod]
        public void MismatchReturnsNoResults()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ReductionDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo", "test.tan").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParametersWinScope()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", TangentType.Void) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo", "test.tan").Select(t => new Identifier(t.Value));

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
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("foo", t) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration("foo", new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("foo", "test.tan").Select(token => new Identifier(token.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(scope.Functions.First(), ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
        }

        [TestMethod]
        public void LazyWorksWithBindings()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { 
                    new ReductionDeclaration(new PhrasePart[] { new PhrasePart("f"), new ParameterDeclaration("x", TangentType.Void.Lazy) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) ,
                    new ReductionDeclaration(new PhrasePart[]{ new PhrasePart("g")}, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())))
                },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = Tokenize.ProgramFile("f g", "test.tan").Select(t => new Identifier(t.Value));

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(ExpressionNodeType.FunctionBinding, ((FunctionInvocationExpression)result.First()).Bindings.Arguments.First().NodeType);
        }

        //[TestMethod]
        //public void LazyWorksWithParams()
        //{
        //    var scope = new Scope(
        //        TangentType.Void,
        //        Enumerable.Empty<TypeDeclaration>(),
        //        new[] { new ParameterDeclaration("g", TangentType.String) },
        //        Enumerable.Empty<ParameterDeclaration>(),
        //        new[] { 
        //            new ReductionDeclaration(new PhrasePart[] { new PhrasePart("f"), new ParameterDeclaration("x", TangentType.String.Lazy) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>())))
        //        });

        //    var tokens = Tokenize.ProgramFile("f g", "test.tan").Select(t => new Identifier(t.Value));

        //    var result = new Input(tokens, scope).InterpretAsStatement();

        //    Assert.AreEqual(1, result.Count);
        //    Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
        //    Assert.AreEqual(ExpressionNodeType.FunctionBinding, ((FunctionInvocationExpression)result.First()).Bindings.Parameters.First().NodeType);
        //}

        [TestMethod]
        public void BuiltinBasicPath()
        {
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                BuiltinFunctions.All,
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("print", null), new ConstantExpression<string>(TangentType.String, "foo", null) }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            Assert.AreEqual(BuiltinFunctions.PrintString, ((FunctionInvocationExpression)result.First()).Bindings.FunctionDefinition);
            Assert.AreEqual(ExpressionNodeType.Constant, ((FunctionInvocationExpression)result.First()).Bindings.Arguments.First().NodeType);
            Assert.AreEqual("foo", ((ConstantExpression<string>)((FunctionInvocationExpression)result.First()).Bindings.Arguments.First()).TypedValue);
        }

        [TestMethod]
        public void EnumValueBasicPath()
        {
            Identifier bar = "bar";
            var foo = new EnumType(new[] { bar });
            var foodecl = new TypeDeclaration("foo", foo);
            var scope = new Scope(
                TangentType.Void,
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo.SingleValueTypeFor(bar))), new Function(TangentType.Void, null)) },
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("bar", null) }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void SpecializationWins()
        {
            Identifier bar = "bar";
            var foo = new EnumType(new[] { bar });
            var foodecl = new TypeDeclaration("foo", foo);

            var special = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo.SingleValueTypeFor(bar))), new Function(TangentType.Void, null));
            var generic = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("p", foo)), new Function(TangentType.Void, null));
            var scope = new Scope(
                TangentType.Void,
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { special, generic },
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("bar", null) }, scope).InterpretAsStatement();

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
                TangentType.Void,
                new[] { foodecl },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { generic },
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("bar", null) }, scope).InterpretAsStatement();

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
                TangentType.Void,
                new[] { booleanDecl },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { ifTrue, ifFalse },
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("if", null), new IdentifierExpression("true", null), new FunctionBindingExpression(new ReductionDeclaration(Enumerable.Empty<PhrasePart>(), new Function(TangentType.Void, null)), new Expression[] { }, null) }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void BasicIfTestFalse()
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
                TangentType.Void,
                new[] { booleanDecl },
                Enumerable.Empty<ParameterDeclaration>(),
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { ifTrue, ifFalse },
                Enumerable.Empty<ParameterDeclaration>());

            var result = new Input(new Expression[] { new IdentifierExpression("if", null), new IdentifierExpression("false", null), new FunctionBindingExpression(new ReductionDeclaration(Enumerable.Empty<PhrasePart>(), new Function(TangentType.Void, null)), new Expression[] { }, null) }, scope).InterpretAsStatement();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void ParenExprInterpretsCorrectly()
        {
            // foo (x: t) => void;
            // (bar: t) => * {
            //    foo (bar);
            // }
            var t = new EnumType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("x", t)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = new List<Expression>() { new IdentifierExpression("foo", null), new ParenExpression(new Block(Enumerable.Empty<Expression>()), new List<Expression>() { new IdentifierExpression("bar", null) }, null) };

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());

            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(1, invoke.Bindings.Arguments.Count());
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, invoke.Bindings.Arguments.First().NodeType);
        }

        [TestMethod]
        public void ParenExprInvokesWhenLazyCorrectly()
        {
            // foo (x: ~>t) => void;
            // (bar: ~>t) => * {
            //    foo (bar);
            // }
            var t = new EnumType(Enumerable.Empty<Identifier>());
            var scope = new Scope(
                TangentType.Void,
                Enumerable.Empty<TypeDeclaration>(),
                new[] { new ParameterDeclaration("bar", t.Lazy) },
                Enumerable.Empty<ParameterDeclaration>(),
                new[] { new ReductionDeclaration(new[] { new PhrasePart(new Identifier("foo")), new PhrasePart(new ParameterDeclaration("x", t.Lazy)) }, new Function(TangentType.Void, new Block(Enumerable.Empty<Expression>()))) },
                Enumerable.Empty<ParameterDeclaration>());

            var tokens = new List<Expression>() { new IdentifierExpression("foo", null), new ParenExpression(new Block(Enumerable.Empty<Expression>()), new List<Expression>() { new IdentifierExpression("bar", null) }, null) };

            var result = new Input(tokens, scope).InterpretAsStatement();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ExpressionNodeType.FunctionInvocation, result.First().NodeType);
            var invoke = ((FunctionInvocationExpression)result.First());

            Assert.AreEqual(scope.Functions.First(), invoke.Bindings.FunctionDefinition);
            Assert.AreEqual(1, invoke.Bindings.Arguments.Count());
            Assert.AreEqual(ExpressionNodeType.FunctionBinding, invoke.Bindings.Arguments.First().NodeType);
        }
    }
}
