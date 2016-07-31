﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PartialLambdaExpressionTests
    {
        [TestMethod]
        public void ParameterCountMismatchReturnsNull()
        {
            var parameter = DelegateType.For(new[] { TangentType.Int, TangentType.Int }, TangentType.Void);
            var lambda = new PartialLambdaExpression(new[] { new ParameterDeclaration("x", null) }, null, (ts, tt) => { Assert.Fail("Should not try to create the lambda."); return new IdentifierExpression("x", null); }, null);

            var result = lambda.TryToFitIn(parameter);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RealParamsAreSentIntoBlock()
        {
            Func<TransformationScope, TangentType, Expression> resolver = (scope, returnType) =>
            {
                Assert.AreEqual(TangentType.Void, returnType);
                var flatRules = scope.Rules.SelectMany(x => x);
                Assert.AreEqual(2, flatRules.Count());
                Assert.IsTrue(flatRules.All(r => r is ParameterAccess));
                var parameters = flatRules.Cast<ParameterAccess>().ToList();
                Assert.IsTrue(parameters.Any(r => r.DeclaredPhrase.Pattern.Count() == 1 && r.DeclaredPhrase.Pattern.First().IsIdentifier && r.DeclaredPhrase.Pattern.First().Identifier.Value == "x"));
                Assert.IsTrue(parameters.Any(r => r.DeclaredPhrase.Pattern.Count() == 1 && r.DeclaredPhrase.Pattern.First().IsIdentifier && r.DeclaredPhrase.Pattern.First().Identifier.Value == "y"));
                Assert.IsTrue(parameters.All(r => r.Reduce(new PhraseMatchResult(1, null, null, null)).EffectiveType == TangentType.Int));

                return null;
            };

            var parameter = DelegateType.For(new[] { TangentType.Int, TangentType.Int }, TangentType.Void);
            var lambda = new PartialLambdaExpression(new[] { new ParameterDeclaration("x", null), new ParameterDeclaration("y", null) }, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())), resolver, null);

            var result = lambda.TryToFitIn(parameter);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AmbiguityLeadsToAmbiguity()
        {
            var ambiguity = new AmbiguousExpression(new[] { new IdentifierExpression("x", null), new IdentifierExpression("x", null) });
            Func<TransformationScope, TangentType, Expression> resolver = (scope, returnType) =>
            {
                Assert.AreEqual(TangentType.Void, returnType);
                var flatRules = scope.Rules.SelectMany(x => x);
                Assert.AreEqual(2, flatRules.Count());
                Assert.IsTrue(flatRules.All(r => r is ParameterAccess));
                var parameters = flatRules.Cast<ParameterAccess>().ToList();
                Assert.IsTrue(parameters.Any(r => r.DeclaredPhrase.Pattern.Count() == 1 && r.DeclaredPhrase.Pattern.First().IsIdentifier && r.DeclaredPhrase.Pattern.First().Identifier.Value == "x"));
                Assert.IsTrue(parameters.Any(r => r.DeclaredPhrase.Pattern.Count() == 1 && r.DeclaredPhrase.Pattern.First().IsIdentifier && r.DeclaredPhrase.Pattern.First().Identifier.Value == "y"));
                Assert.IsTrue(parameters.All(r => r.Reduce(new PhraseMatchResult(1, null, null, null)).EffectiveType == TangentType.Int));

                return ambiguity;
            };

            var parameter = DelegateType.For(new[] { TangentType.Int, TangentType.Int }, TangentType.Void);
            var lambda = new PartialLambdaExpression(new[] { new ParameterDeclaration("x", null), new ParameterDeclaration("y", null) }, new TransformationScope(Enumerable.Empty<TransformationRule>(), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>())), resolver, null);

            var result = lambda.TryToFitIn(parameter);
            Assert.AreEqual(ambiguity, result);
        }
    }
}
