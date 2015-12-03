using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    public class PhrasePriorityComparerTests
    {
        [TestMethod]
        public void LongerPhrasesWin()
        {
            var x = new Phrase("a", "b");
            var y = new Phrase("a", "b", "c");

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) > 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) < 0);
        }

        [TestMethod]
        public void IdentifiersWin()
        {
            var x = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("id", TangentType.Int)) });
            var y = new Phrase("a");

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) > 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) < 0);
        }

        [TestMethod]
        public void SingleValueTypesWin()
        {
            var enumType = new EnumType(new Identifier[] { "a", "e", "i", "o", "u" });
            var svt = enumType.SingleValueTypeFor("i");

            var x = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("input", enumType)) });
            var y = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("eye", svt)) });

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) > 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) < 0);
        }

        [TestMethod]
        public void SingleValueTypesOnlyWinForSameEnum()
        {
            var enumType = new EnumType(new Identifier[] { "a", "e", "i", "o", "u" });
            var otherEnum = new EnumType(new Identifier[]{ "i" });
            var svt = enumType.SingleValueTypeFor("i");

            var x = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("input", otherEnum)) });
            var y = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("eye", svt)) });

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) == 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) == 0);
        }

        [TestMethod]
        public void NonGenericsBeatGenerics()
        {
            var genericParam = new ParameterDeclaration("T", TangentType.Any.Kind);
            var x = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("x", GenericInferencePlaceholder.For(genericParam))) });
            var y = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("y", TangentType.String)) });

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) > 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) < 0);
        }

        [TestMethod]
        [Ignore]
        public void MoreSpecificGenericsWin()
        {
            // This test does not currently pass because CompatibilityMatches is not yet implemented in GenericInferencePlaceholder.
            var anyGeneric = new ParameterDeclaration("T", TangentType.Any.Kind);
            var stringGeneric = new ParameterDeclaration("R", TangentType.String.Kind);
            var x = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("x", GenericInferencePlaceholder.For(anyGeneric))) });
            var y = new Phrase(new[] { new PhrasePart(new ParameterDeclaration("y", GenericInferencePlaceholder.For(stringGeneric))) });

            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(x, y) > 0);
            Assert.IsTrue(PhrasePriorityComparer.ComparePriority(y, x) < 0);
        }
    }
}
