using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;


namespace Tangent.CilGeneration
{
    public class CilCompiler
    {
        private readonly Dictionary<object, string> names = new Dictionary<object, string>();

        public void Compile(TangentProgram program, string targetPath)
        {
            var entrypoint = program.Functions.FirstOrDefault(
                fn => fn.Takes.Count == 1 &&
                    fn.Takes.First().IsIdentifier &&
                    fn.Takes.First().Identifier.Value == "entrypoint" &&
                    fn.Returns.EffectiveType == TangentType.Void);

            string filename = Path.GetFileNameWithoutExtension(targetPath);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName(filename), AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(filename + (entrypoint == null ? ".dll" : ".exe"), Path.GetFileName(targetPath) + (entrypoint == null ? ".dll" : ".exe"), true);

            Dictionary<string, ISymbolDocumentWriter> debuggingSymbolLookup = program.InputLabels.ToDictionary(l => l, l => moduleBuilder.DefineDocument(l, Guid.Empty, Guid.Empty, Guid.Empty));
            using (var typeLookup = new DelegatingTypeLookup(new CilTypeCompiler(moduleBuilder), program.TypeDeclarations, AppDomain.CurrentDomain)) {

                var rootClass = moduleBuilder.DefineType("_");
                var fnLookup = new CilScope(rootClass, program.Functions, typeLookup);
                var compiler = new CilFunctionCompiler(BuiltinFunctionLookup.Common, debuggingSymbolLookup);

                typeLookup.BakeTypes();
                fnLookup.Compile(compiler);

                rootClass.CreateType();

                if (entrypoint != null) {
                    var entrypointMethod = fnLookup[entrypoint];
                    assemblyBuilder.SetEntryPoint(entrypointMethod);
                }
            }

            assemblyBuilder.Save(targetPath + (entrypoint == null ? ".dll" : ".exe"));
        }
    }
}
