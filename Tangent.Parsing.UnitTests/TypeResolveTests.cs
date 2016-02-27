using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.UnitTests
{
    [TestClass]
    public class TypeResolveTests
    {
        private static IEnumerable<IdentifierExpression> Fix(IEnumerable<Identifier> ids)
        {
            return ids.Select(id => new IdentifierExpression(id, null));
        }

        [TestMethod]
        public void BasicTypeResolve()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve1()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve2()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void PhraseTypeResolve()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);
        }

        [TestMethod]
        public void PhraseTypeResolveOrderMatters()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "bar", "foo" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void PhraseTypeResolvePicksFromMany()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new[]{
                new TypeDeclaration(new Identifier[] { "foo", "baz" }, new EnumType(new Identifier[0])),
                new TypeDeclaration(new Identifier[] { "foo", "baa" }, new EnumType(new Identifier[0])),
                new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo),
                new TypeDeclaration(new Identifier[] { "foo", "bar", "baz" }, new EnumType(new Identifier[0]))};


            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar" }), typeDecl, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);

            Assert.IsTrue(TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", "baz" }), typeDecl, Enumerable.Empty<ParameterDeclaration>()).Success);
            Assert.AreNotEqual(foo, TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", "baz" }), typeDecl, Enumerable.Empty<ParameterDeclaration>()).Result);
        }

        [TestMethod]
        public void ParameterResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration(new IdentifierExpression("x", null), new List<Expression>() { new IdentifierExpression("foo", null), new IdentifierExpression("bar", null) });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result.Returns);
            Assert.AreEqual(new Identifier("x"), result.Result.Takes.First().Identifier);
        }


        [TestMethod]
        public void ParameterResolutionSadPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration(new IdentifierExpression("x", null), new List<Expression>() { new IdentifierExpression("foo", null), new IdentifierExpression("baz", null) });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FunctionResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialReductionDeclaration(
                new PartialPhrasePart(new PartialParameterDeclaration(new IdentifierExpression("x", null), new List<Expression>() { new IdentifierExpression("foo", null), new IdentifierExpression("bar", null) })),
                new PartialFunction(Fix(new Identifier[] { "foo", "bar" }), new PartialBlock(Enumerable.Empty<PartialStatement>()), null));

            var result = TypeResolve.PartialFunctionDeclaration(partial, new[] { typeDecl }, new Dictionary<TangentType, TangentType>());

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Result.Takes.First().IsIdentifier);

            var x = result.Result.Takes.First().Parameter;
            Assert.AreEqual(foo, x.Returns);
            Assert.AreEqual(new Identifier("x"), x.Takes.First().Identifier);

            Assert.AreEqual(foo, result.Result.Returns.EffectiveType);
        }

        [TestMethod]
        public void SingleValueResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", ".", "a" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.SingleValue, result.Result.ImplementationType);
            var svt = result.Result as SingleValueType;
            Assert.AreEqual(foo, svt.ValueType);
            Assert.AreEqual("a", svt.Value);

            result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", ".", "moocow" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.SingleValue, result.Result.ImplementationType);
            svt = result.Result as SingleValueType;
            Assert.AreEqual(foo, svt.ValueType);
            Assert.AreEqual("moocow", svt.Value);
        }

        [TestMethod]
        public void SingleValueResolutionTwoDotsDies()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", ".", ".", "a" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsFalse(result.Success);
        }


        [TestMethod]
        public void SingleValueResolutionIdMismatchFails()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "bar", ".", "moo" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void SingleValueResolutionTypeMismatchFails()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "foo", "baa", ".", "moocow" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void LazyHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "~>", "foo", "bar" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Delegate, result.Result.ImplementationType);
            Assert.AreEqual(foo.Lazy, result.Result);
        }

        [TestMethod]
        public void SuperLazyHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "~>", "~>", "foo", "bar" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Delegate, result.Result.ImplementationType);
            Assert.AreEqual(foo.Lazy.Lazy, result.Result);
        }

        [TestMethod]
        public void LazyWithDots()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "~>", "foo", "bar", ".", "moocow" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Delegate, result.Result.ImplementationType);
            Assert.AreEqual(foo.SingleValueTypeFor("moocow").Lazy, result.Result);
        }


        [TestMethod]
        public void LazyResolutionFailsGracefully()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(Fix(new Identifier[] { "~>", "foo", "bah" }), new[] { typeDecl }, Enumerable.Empty<ParameterDeclaration>());
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void GenericResolutionWorks()
        {
            var result = TypeResolve.ResolveType(new IdentifierExpression[] { new IdentifierExpression("T", null) }, new TypeDeclaration[0], new ParameterDeclaration[] { new ParameterDeclaration("T", TangentType.Int.Kind) });

            Assert.IsTrue(result.Success);
            var genRef = result.Result as GenericArgumentReferenceType;
            Assert.IsNotNull(genRef);
            Assert.AreEqual(TangentType.Int.Kind, genRef.GenericParameter.Returns);
        }

        [TestMethod]
        public void GenericResolutionHigherPriorityThanTypes()
        {
            var result = TypeResolve.ResolveType(
                new IdentifierExpression[] { new IdentifierExpression("T", null) },
                new TypeDeclaration[] { new TypeDeclaration("T", TangentType.Int) },
                new ParameterDeclaration[] { new ParameterDeclaration("T", TangentType.Int.Kind) });

            Assert.IsTrue(result.Success);
            var genRef = result.Result as GenericArgumentReferenceType;
            Assert.IsNotNull(genRef);
            Assert.AreEqual(TangentType.Int.Kind, genRef.GenericParameter.Returns);
        }
    }
}
