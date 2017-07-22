using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        public static string DebugProgramFile(IEnumerable<string> paths, IEnumerable<Assembly> imports = null, string input = null)
        {
            var targetExe = Path.GetFileNameWithoutExtension(paths.First()) + ".exe";
            var args = paths.ToList();

            // Copy Pastey from Cli.Program
            IEnumerable<Token> tokenization = Enumerable.Empty<Token>();

            for (int x = 0; x == 0 || x < args.Count; ++x) {
                tokenization = tokenization.Concat(Tokenize.ProgramFile(File.ReadAllText(args[x]), args[x]));
            }

            HashSet<string> tokenValues = new HashSet<string>(tokenization.Select(t => t.Value).Where(v => v != null));
            var intermediateProgram = Parse.TangentProgram(tokenization, TangentImport.ImportAssemblies(imports ?? Enumerable.Empty<Assembly>(), s => tokenValues.Contains(s)));
            if (!intermediateProgram.Success) {
                Assert.Fail(string.Format("Errors during compilation: {0}", intermediateProgram.Error));
            }

            //var compiler = new CilCompiler();
            //compiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First()));
            var compilation = Task.Run(() => NewCilCompiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First())));
            if (compilation.Wait(TimeSpan.FromSeconds(30))) { } else { Assert.Fail("Compilation timeout."); }

            var discard = TimeSpan.Zero;
            return RunCompiledProgram(targetExe, out discard, input);
        }

        public static string DebugProgramFile(string path, IEnumerable<Assembly> imports = null, string input = null)
        {
            return DebugProgramFile(new[] { path }, imports, input);
        }

        public static string DebugProgramFile(IEnumerable<string> path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null, string input = null)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path, imports, input);
        }

        public static string DebugProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null, string input = null)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path, imports, input);
        }

        public static string ProgramFile(string path, IEnumerable<Assembly> imports = null, string input = null)
        {
            var discard = TimeSpan.Zero;
            return ProgramFile(path, out discard, out discard, imports, input);
        }

        public static string ProgramFile(IEnumerable<string> paths, IEnumerable<Assembly> imports = null, string input = null)
        {
            TimeSpan discard;
            return ProgramFile(paths, out discard, out discard, imports, input);
        }

        public static string ProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null, string input = null)
        {
            return ProgramFile(new[] { path }, out compileDuration, out programDuration, imports, input);
        }

        public static string ProgramFile(IEnumerable<string> paths, out TimeSpan compileDuration, out TimeSpan programDuration, IEnumerable<Assembly> imports = null, string input = null)
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

            if (!compileProcess.WaitForExit(500000)) {
                Assert.Fail("Compilation timed out...");
            }

            compileTimer.Stop();
            compileDuration = compileTimer.Elapsed;
            Debug.WriteLine(string.Format("Path: {0}, Compile time: {1}", paths, compileTimer.Elapsed));

            var compileErrors = compileProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(compileErrors)) {
                Assert.Fail(string.Format("Compilation errors: {0}", compileErrors));
            }

            return RunCompiledProgram(targetExe, out programDuration, input);
        }

        private static string RunCompiledProgram(string targetExe, out TimeSpan programDuration, string input)
        {
            input = input ?? "";
            Assert.IsTrue(File.Exists(targetExe), "Exe wasn't generated?");

            var programProcess = new Process();

            programProcess.StartInfo.UseShellExecute = false;
            programProcess.StartInfo.RedirectStandardOutput = true;
            programProcess.StartInfo.RedirectStandardError = true;
            programProcess.StartInfo.RedirectStandardInput = true;
            programProcess.StartInfo.FileName = targetExe;

            var programTimer = Stopwatch.StartNew();
            if (!programProcess.Start()) {
                Assert.Fail("Compiled program would not start.");
            }

            programProcess.StandardInput.Write(input);

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
