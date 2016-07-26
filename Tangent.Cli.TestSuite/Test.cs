using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tangent.CilGeneration;
using Tangent.Intermediate.Interop;
using Tangent.Parsing;
using Tangent.Tokenization;

namespace Tangent.Cli.TestSuite
{
    [ExcludeFromCodeCoverage]
    public static class Test
    {
        public static string DebugProgramFile(IEnumerable<string> paths, IEnumerable<Assembly> imports = null)
        {
            var targetExe = Path.GetFileNameWithoutExtension(paths.First()) + ".exe";
            var args = paths.ToList();

            // Copy Pastey from Cli.Program
            IEnumerable<Token> tokenization = Enumerable.Empty<Token>();

            for (int x = 0; x == 0 || x < args.Count; ++x) {
                tokenization = tokenization.Concat(Tokenize.ProgramFile(File.ReadAllText(args[x]), args[x]));
            }

            var intermediateProgram = Parse.TangentProgram(tokenization, TangentImport.ImportAssemblies(imports ?? Enumerable.Empty<Assembly>()));
            if (!intermediateProgram.Success) {
                Assert.Fail(string.Format("Errors during compilation: {0}", intermediateProgram.Error));
            }

            //var compiler = new CilCompiler();
            //compiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First()));

            NewCilCompiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First()));

            var discard = TimeSpan.Zero;
            return RunCompiledProgram(targetExe, out discard);
        }

        public static string DebugProgramFile(string path, IEnumerable<Assembly> imports = null)
        {
            return DebugProgramFile(new[] { path }, imports);
        }

        public static string DebugProgramFile(IEnumerable<string> path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path, imports);
        }

        public static string DebugProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path, imports);
        }

        public static string ProgramFile(string path, IEnumerable<Assembly> imports = null)
        {
            var discard = TimeSpan.Zero;
            return ProgramFile(path, out discard, out discard, imports);
        }

        public static string ProgramFile(IEnumerable<string> paths, IEnumerable<Assembly> imports = null)
        {
            TimeSpan discard;
            return ProgramFile(paths, out discard, out discard, imports);
        }

        public static string ProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null)
        {
            return ProgramFile(new[] { path }, out compileDuration, out programDuration, imports);
        }

        public static string ProgramFile(IEnumerable<string> paths, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null)
        {
            var compileProcess = new Process();
            var targetExe = Path.GetFileNameWithoutExtension(paths.First()) + ".exe";
            var targetConfig = Path.GetFileNameWithoutExtension(paths.First()) + ".tanbuild";

            var config = new CompilerInputs1() {
                SourceFiles = new HashSet<string>(paths),
                DestinationFile = targetExe,
                DllImports = new HashSet<string>((imports ?? Enumerable.Empty<Assembly>()).Select(assembly => assembly.FullName))
            };

            File.WriteAllText(targetConfig, JsonConvert.SerializeObject(config, Formatting.Indented));

            compileProcess.StartInfo.UseShellExecute = false;
            compileProcess.StartInfo.CreateNoWindow = true;
            compileProcess.StartInfo.RedirectStandardError = true;
            compileProcess.StartInfo.Arguments = "-f " + targetConfig;
            compileProcess.StartInfo.FileName = "Tangent.Cli.exe";

            var compileTimer = Stopwatch.StartNew();
            if (!compileProcess.Start()) {
                Assert.Fail("Process could not start.");
            }

            if (!compileProcess.WaitForExit(5000000)) {
                Assert.Fail("Compilation timed out...");
            }

            compileTimer.Stop();
            compileDuration = compileTimer.Elapsed;
            Debug.WriteLine(string.Format("Path: {0}, Compile time: {1}", paths, compileTimer.Elapsed));

            var compileErrors = compileProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(compileErrors)) {
                Assert.Fail(string.Format("Compilation errors: {0}", compileErrors));
            }

            return RunCompiledProgram(targetExe, out programDuration);
        }

        private static string RunCompiledProgram(string targetExe, out TimeSpan programDuration)
        {

            Assert.IsTrue(File.Exists(targetExe), "Exe wasn't generated?");

            var programProcess = new Process();

            programProcess.StartInfo.UseShellExecute = false;
            programProcess.StartInfo.RedirectStandardOutput = true;
            programProcess.StartInfo.RedirectStandardError = true;
            programProcess.StartInfo.FileName = targetExe;

            var programTimer = Stopwatch.StartNew();
            if (!programProcess.Start()) {
                Assert.Fail("Compiled program would not start.");
            }

            if (!programProcess.WaitForExit(5000)) {
                Assert.Fail("Program execution timed out.");
            }

            programTimer.Stop();
            Debug.WriteLine(string.Format("Path: {0}, Runtime: {1}", targetExe, programTimer.Elapsed));

            programDuration = programTimer.Elapsed;
            var programOutput = programProcess.StandardOutput.ReadToEnd();
            var programError = programProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(programError)) {
                Assert.Fail(string.Format("Compiled program produced errors: {0}", programError));
            }

            return programOutput;
        }
    }
}
