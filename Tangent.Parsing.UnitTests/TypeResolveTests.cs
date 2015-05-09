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
        [TestMethod]
        public void BasicTypeResolve()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo" }, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve1()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, new[] { typeDecl });

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve2()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo" }, new[] { typeDecl });

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void PhraseTypeResolve()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, new[] { typeDecl });
            
            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);
        }

        [TestMethod]
        public void PhraseTypeResolveOrderMatters()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "bar", "foo" }, new[] { typeDecl });

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


            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, typeDecl);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result);

            Assert.IsTrue(TypeResolve.ResolveType(new Identifier[] { "foo", "bar", "baz" }, typeDecl).Success);
            Assert.AreNotEqual(foo, TypeResolve.ResolveType(new Identifier[] { "foo", "bar", "baz" }, typeDecl).Result);
        }

        [TestMethod]
        public void ParameterResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "bar" });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result.Returns);
            Assert.AreEqual(new Identifier("x"), result.Result.Takes.First());
        }


        [TestMethod]
        public void ParameterResolutionSadPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "baz" });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FunctionResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialReductionDeclaration(
                new PartialPhrasePart(new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "bar" })),
                new PartialFunction(new Identifier[] { "foo", "bar" }, new PartialBlock(Enumerable.Empty<PartialStatement>()), null));

            var result = TypeResolve.PartialFunctionDeclaration(partial, new[] { typeDecl }, new Dictionary<PartialProductType,ProductType>());

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Result.Takes.First().IsIdentifier);

            var x = result.Result.Takes.First().Parameter;
            Assert.AreEqual(foo, x.Returns);
            Assert.AreEqual(new Identifier("x"), x.Takes.First());

            Assert.AreEqual(foo, result.Result.Returns.EffectiveType);
        }

        [TestMethod]
        public void SingleValueResolutionHappyPath()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar", ".", "a" }, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.SingleValue, result.Result.ImplementationType);
            var svt = result.Result as SingleValueType;
            Assert.AreEqual(foo, svt.ValueType);
            Assert.AreEqual("a", svt.Value);

            result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar", ".", "moocow" }, new[] { typeDecl });

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

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar", ".", ".", "a" }, new[] { typeDecl });
            Assert.IsFalse(result.Success);
        }


        [TestMethod]
        public void SingleValueResolutionIdMismatchFails()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar", ".", "moo" }, new[] { typeDecl });
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void SingleValueResolutionTypeMismatchFails()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "baa", ".", "moocow" }, new[] { typeDecl });
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void LazyHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "~>", "foo", "bar" }, new[] { typeDecl });
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Lazy, result.Result.ImplementationType);
            Assert.AreEqual(foo.Lazy, result.Result);
        }

        [TestMethod]
        public void SuperLazyHappyPath()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "~>", "~>", "foo", "bar" }, new[] { typeDecl });
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Lazy, result.Result.ImplementationType);
            Assert.AreEqual(foo.Lazy.Lazy, result.Result);
        }

        [TestMethod]
        public void LazyWithDots()
        {
            var foo = new EnumType(new Identifier[] { "a", "moocow" });
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "~>", "foo", "bar", ".", "moocow"}, new[] { typeDecl });
            Assert.IsTrue(result.Success);
            Assert.AreEqual(KindOfType.Lazy, result.Result.ImplementationType);
            Assert.AreEqual(foo.SingleValueTypeFor("moocow").Lazy, result.Result);
        }


        [TestMethod]
        public void LazyResolutionFailsGracefully()
        {
            var foo = new EnumType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "~>", "foo", "bah" }, new[] { typeDecl });
            Assert.IsFalse(result.Success);
        }
    }
}
