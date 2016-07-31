using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PhraseTests
    {
        [TestMethod]
        public void HappyPath()
        {
            var genericT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var phrase = new Phrase(new[]{
                new PhrasePart(new Identifier("foo")),
                new PhrasePart(new ParameterDeclaration("x", TangentType.Int)),
                new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(genericT)))});

            var input = new Expression[]{ 
                new IdentifierExpression(new Identifier("foo"), null),
                new ConstantExpression<int>(TangentType.Int, 42, null),
                new ConstantExpression<string>(TangentType.String, "moo", null)};

            var result = phrase.TryMatch(input, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.TokenMatchLength);
            Assert.AreEqual(2, result.IncomingArguments.Count());
            Assert.AreEqual(1, result.GenericInferences.Count());

            Assert.AreEqual(genericT, result.GenericInferences.First().Key);
            Assert.AreEqual(TangentType.String, result.GenericInferences.First().Value);

            Assert.IsTrue(result.IncomingArguments.First() is ConstantExpression<int>);
            Assert.IsTrue(result.IncomingArguments.Skip(1).First() is ConstantExpression<string>);
        }

        [TestMethod]
        public void PartialMatchesAreFine()
        {
            var genericT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var phrase = new Phrase(new[]{
                new PhrasePart(new Identifier("foo")),
                new PhrasePart(new ParameterDeclaration("x", TangentType.Int)),
                new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(genericT)))});

            var input = new Expression[]{ 
                new IdentifierExpression(new Identifier("foo"), null),
                new ConstantExpression<int>(TangentType.Int, 42, null),
                new ConstantExpression<string>(TangentType.String, "moo", null),
                new IdentifierExpression(new Identifier("fooooo"), null)
            };

            var result = phrase.TryMatch(input, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.TokenMatchLength);
            Assert.AreEqual(2, result.IncomingArguments.Count());
            Assert.AreEqual(1, result.GenericInferences.Count());

            Assert.AreEqual(genericT, result.GenericInferences.First().Key);
            Assert.AreEqual(TangentType.String, result.GenericInferences.First().Value);

            Assert.IsTrue(result.IncomingArguments.First() is ConstantExpression<int>);
            Assert.IsTrue(result.IncomingArguments.Skip(1).First() is ConstantExpression<string>);
        }

        [TestMethod]
        public void ShortInputFailsGracefully()
        {
            var genericT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var phrase = new Phrase(new[]{
                new PhrasePart(new Identifier("foo")),
                new PhrasePart(new ParameterDeclaration("x", TangentType.Int)),
                new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(genericT)))});

            var input = new Expression[]{ 
                new IdentifierExpression(new Identifier("foo"), null),
                new ConstantExpression<int>(TangentType.Int, 42, null)};

            var result = phrase.TryMatch(input, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())));

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ParamMismatchFailsGracefully()
        {
            var genericT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var phrase = new Phrase(new[]{
                new PhrasePart(new Identifier("foo")),
                new PhrasePart(new ParameterDeclaration("x", TangentType.Int)),
                new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(genericT)))});

            var input = new Expression[]{ 
                new IdentifierExpression(new Identifier("foo"), null),
                new ConstantExpression<string>(TangentType.String, "42", null),
                new ConstantExpression<string>(TangentType.String, "moo", null)};

            var result = phrase.TryMatch(input, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())));

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ParamMismatchFailsGracefully2()
        {
            var genericT = new ParameterDeclaration("T", TangentType.Any.Kind);
            var phrase = new Phrase(new[]{
                new PhrasePart(new Identifier("foo")),
                new PhrasePart(new ParameterDeclaration("x", TangentType.Int)),
                new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(genericT)))});

            var input = new Expression[]{ 
                new IdentifierExpression(new Identifier("foo"), null),
                new ConstantExpression<int>(TangentType.Int, 42, null),
                new IdentifierExpression(new Identifier("foo"), null)};

            var result = phrase.TryMatch(input, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())));

            Assert.IsFalse(result.Success);
        }
    }
}
