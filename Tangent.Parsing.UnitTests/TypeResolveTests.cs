using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing.UnitTests {
    [TestClass]
    public class TypeResolveTests {

        [TestMethod]
        public void BasicTypeResolve() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo" }, new[] { typeDecl });

            Assert.AreEqual(foo, result);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve1() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration("foo", foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, new[] { typeDecl });

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FullMatchRequiredTypeResolve2() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo" }, new[] { typeDecl });

            Assert.IsNull(result);
        }

        [TestMethod]
        public void PhraseTypeResolve() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, new[] { typeDecl });

            Assert.AreEqual(foo, result);
        }

        [TestMethod]
        public void PhraseTypeResolveOrderMatters() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var result = TypeResolve.ResolveType(new Identifier[] { "bar", "foo" }, new[] { typeDecl });

            Assert.IsNull(result);
        }

        [TestMethod]
        public void PhraseTypeResolvePicksFromMany() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new[]{
                new TypeDeclaration(new Identifier[] { "foo", "baz" }, new TangentType(new Identifier[0])),
                new TypeDeclaration(new Identifier[] { "foo", "baa" }, new TangentType(new Identifier[0])),
                new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo),
                new TypeDeclaration(new Identifier[] { "foo", "bar", "baz" }, new TangentType(new Identifier[0]))};


            var result = TypeResolve.ResolveType(new Identifier[] { "foo", "bar" }, typeDecl);

            Assert.AreEqual(foo, result);

            Assert.IsNotNull(TypeResolve.ResolveType(new Identifier[] { "foo", "bar", "baz" }, typeDecl));
            Assert.AreNotEqual(foo, TypeResolve.ResolveType(new Identifier[] { "foo", "bar", "baz" }, typeDecl));
        }

        [TestMethod]
        public void ParameterResolutionHappyPath() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "bar" });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(foo, result.Result.EndResult());
            Assert.AreEqual(new Identifier("x"), result.Result.Takes);
        }


        [TestMethod]
        public void ParameterResolutionSadPath() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "baz" });
            var result = TypeResolve.Resolve(partial, new[] { typeDecl });

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void FunctionResolutionHappyPath() {
            var foo = new TangentType(new Identifier[0]);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, foo);

            var partial = new PartialReductionDeclaration(
                new PartialPhrasePart(new PartialParameterDeclaration("x", new List<Identifier>() { "foo", "bar" })),
                new PartialFunction(new Identifier[] { "foo", "bar" }, new PartialBlock(Enumerable.Empty<PartialStatement>())));

            var result = TypeResolve.PartialFunctionDeclaration(partial, new[] { typeDecl });

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Result.Takes.IsIdentifier);

            var x = result.Result.Takes.Parameter;
            Assert.AreEqual(foo, x.EndResult());
            Assert.AreEqual(new Identifier("x"), x.Takes);

            Assert.AreEqual(foo, result.Result.EndResult().EffectiveType);
        }
    }
}
