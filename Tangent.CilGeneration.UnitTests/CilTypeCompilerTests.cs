using System;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.Intermediate;

namespace Tangent.CilGeneration.UnitTests
{
    [TestClass]
    public class CilTypeCompilerTests
    {
        [TestMethod]
        public void GetNameForTypeHappyPath()
        {
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, TangentType.Void);
            var result = CilTypeCompiler.GetNameFor(typeDecl);
            Assert.AreEqual("foo bar", result);
        }

        [TestMethod]
        public void GetNameForTypeLazyPath()
        {
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, TangentType.Void.Lazy);
            var result = CilTypeCompiler.GetNameFor(typeDecl);

            Assert.AreEqual("~> foo bar", result);
        }

        [TestMethod]
        public void GetNameForTypeSuperLazyPath()
        {
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, TangentType.Void.Lazy.Lazy.Lazy);
            var result = CilTypeCompiler.GetNameFor(typeDecl);

            Assert.AreEqual("~> ~> ~> foo bar", result);
        }

        [TestMethod]
        public void BuildEnumHappyPath()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("BuildEnumHappyPath"), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("BuildEnumHappyPath");
            var compiler = new CilTypeCompiler(module);
            var typeDecl = new TypeDeclaration(new Identifier[] { "foo", "bar" }, new EnumType(new Identifier[] { "a", "b", "c" }));

            var result = compiler.Compile(typeDecl);
            Assert.AreEqual("foo bar", result.Name);
            var values = Enum.GetValues(result);
            Assert.AreEqual(3, values.Length);
            Assert.IsTrue(values.Cast<object>().Select(v => v.ToString()).OrderBy(x => x).SequenceEqual(new[] { "a", "b", "c" }));
            Assert.IsTrue(module.GetTypes().Contains(result));
        }
    }
}
