using System;
using System.Collections.Generic;
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
        private readonly DebugInfoGenerator pdb = DebugInfoGenerator.CreatePdbGenerator();

        public void Compile(TangentProgram program, string targetPath)
        {
            string filename = Path.GetFileNameWithoutExtension(targetPath);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName(filename), AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(filename + ".dll", Path.GetFileName(targetPath) + ".dll", true);

            ITypeLookup typeLookup = new DelegatingTypeLookup(new CilTypeCompiler(moduleBuilder), program.TypeDeclarations);

            var rootClass = moduleBuilder.DefineType("_");
            var fnLookup = new CilScope(rootClass, program.Functions, typeLookup);
            var compiler = new CilFunctionCompiler(EmptyFunctionLookup.Common);

            fnLookup.Compile(compiler);

            rootClass.CreateType();

            assemblyBuilder.Save(targetPath + ".dll");
        }
    }
}
