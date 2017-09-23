using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Tangent.Intermediate.Interop.UnitTests
{
    public struct TestTarget { }

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TangentImportTests
    {
        [TestMethod]
        public void TypeDeclarationParsesNameProperly()
        {
            var td = DotNetType.TypeDeclarationFor(typeof(TestTarget));
            Assert.AreEqual(11, td.Takes.Count);
            Assert.AreEqual(".", td.Takes.First().Identifier.Value);
            Assert.AreEqual("NET", td.Takes.Skip(1).First().Identifier.Value);
            Assert.AreEqual("Tangent", td.Takes.Skip(2).First().Identifier.Value);
            Assert.AreEqual(".", td.Takes.Skip(3).First().Identifier.Value);
            Assert.AreEqual("Intermediate", td.Takes.Skip(4).First().Identifier.Value);
            Assert.AreEqual(".", td.Takes.Skip(5).First().Identifier.Value);
            Assert.AreEqual("Interop", td.Takes.Skip(6).First().Identifier.Value);
            Assert.AreEqual(".", td.Takes.Skip(7).First().Identifier.Value);
            Assert.AreEqual("UnitTests", td.Takes.Skip(8).First().Identifier.Value);
            Assert.AreEqual(".", td.Takes.Skip(9).First().Identifier.Value);
            Assert.AreEqual("TestTarget", td.Takes.Skip(10).First().Identifier.Value);
        }

        [TestMethod]
        public void ImportSystem()
        {
            var timer = Stopwatch.StartNew();
            var result = TangentImport.ImportAssembly(typeof(int).Assembly, x => true, new NoopCompilerTimings());
            timer.Stop();
            Console.WriteLine("Import Complete.");
            Console.WriteLine("Imported Types ({0}):", result.Types.Count);
            foreach (var entry in result.Types) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine("");
            Console.WriteLine("Imported Constructors ({0}):", result.Constructors.Count + result.StructInits.Count);
            foreach (var entry in result.Constructors) {
                Console.WriteLine("  {0}", entry.Value);
            }

            foreach (var entry in result.StructInits) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Functions ({0}):", result.CommonFunctions.Count);
            foreach (var entry in result.CommonFunctions) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Field Accessors ({0}):", result.FieldAccessors.Count);
            foreach (var entry in result.FieldAccessors) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Field Assignments ({0}):", result.FieldMutators.Count);
            foreach (var entry in result.FieldMutators) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Interface Bindings ({0}):", result.InterfaceBindings.Count);
            foreach (var entry in result.InterfaceBindings) {
                Console.WriteLine("  {0}", entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Elapsed Time: {0}", timer.Elapsed);
        }

        [TestMethod]
        public void BulkImport()
        {
            var timer = Stopwatch.StartNew();
            var result = TangentImport.ImportAssemblies(new[] { typeof(int).Assembly, typeof(List<>).Assembly, typeof(Enumerable).Assembly, typeof(IEnumerable<>).Assembly, typeof(IEnumerator<>).Assembly }.Distinct(), x => true);
            timer.Stop();

            Console.WriteLine("Import Complete.");
            Console.WriteLine("Imported Types ({0}):", result.TypeDeclarations.Count());
            foreach (var entry in result.TypeDeclarations) {
                Console.WriteLine("  {0}", entry);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Functions ({0}):", result.Functions.Count());
            foreach (var entry in result.Functions) {
                Console.WriteLine("  {0}", entry);
            }

            Console.WriteLine();
            Console.WriteLine("Imported Interface Bindings ({0}):", result.InterfaceBindings.Count());
            foreach (var entry in result.InterfaceBindings) {
                Console.WriteLine("  {0}", entry);
            }

            Console.WriteLine();
            Console.WriteLine("Elapsed Time: {0}", timer.Elapsed);
        }
    }
}
