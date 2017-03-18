using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TransformationScopeTests
    {
        [TestMethod]
        public void EmptyPrioritizationIsGraceful()
        {
            var result = TransformationScopeOld.Prioritize(Enumerable.Empty<TransformationRule>());

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void RuleOrderIsHeld()
        {
            var rules = Enum.GetValues(typeof(TransformationType)).Cast<TransformationType>().Select(tt =>
            {
                var mockRule = new Mock<TransformationRule>();
                mockRule.Setup(r => r.Type).Returns(tt);
                mockRule.Setup(r => r.MaxTakeCount).Returns(1);
                return mockRule.Object;
            }).ToList();

            var rng = new Random();
            rules = rules.OrderBy(x => rng.Next()).ToList();

            var result = TransformationScopeOld.Prioritize(rules).ToList();

            Assert.AreEqual(rules.Count, result.Count());
            Assert.IsTrue(result.All(x => x.Count() == 1));
            var flatResult = result.Select(x => x.First());
            var orderedRules = rules.OrderBy(x => (int)x.Type);

            Assert.IsTrue(flatResult.SequenceEqual(orderedRules));
        }

        [TestMethod]
        public void TakeCountIsOrderedDescending()
        {
            var rules = Enumerable.Range(1, 4).Select(ct =>
            {
                var mockRule = new Mock<TransformationRule>();
                mockRule.Setup(r => r.Type).Returns(TransformationType.Function);
                mockRule.Setup(r => r.MaxTakeCount).Returns(ct);
                return mockRule.Object;
            });

            var ordered = rules.Reverse();

            var result = TransformationScopeOld.Prioritize(rules);
            Assert.AreEqual(4, result.Count());
            Assert.IsTrue(result.All(r => r.Count() == 1));
            var flatResult = result.Select(x => x.First());
            Assert.IsTrue(flatResult.Select(x => x.MaxTakeCount).SequenceEqual(ordered.Select(x => x.MaxTakeCount)));
        }

        [TestMethod]
        public void ExpressionOrdering()
        {
            var x = new ParameterAccess(new ParameterDeclaration("x", TangentType.Int));
            var xy = new ParameterAccess(new ParameterDeclaration(new Identifier[] { "x", "y" }, TangentType.Int));
            var rules = new TransformationRule[]{x,xy};

            var result = TransformationScopeOld.Prioritize(rules);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(r => r.Count() == 1));

            var flatResult = result.Select(r => (ParameterAccess)r.First());
            Assert.AreEqual(2, flatResult.First().DeclaredPhrase.Pattern.Count());
            Assert.AreEqual(1, flatResult.Skip(1).First().DeclaredPhrase.Pattern.Count());
        }
    }
}
