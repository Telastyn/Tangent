using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tangent.CilGeneration;
using Tangent.Parsing;
using Tangent.Tokenization;

namespace Tangent.Cli.TestSuite
{
    [ExcludeFromCodeCoverage]
    public static class Test
    {
        public static string DebugProgramFile(IEnumerable<string> paths)
        {
            var targetExe = Path.GetFileNameWithoutExtension(paths.First()) + ".exe";
            var args = paths.ToList();

            // Copy Pastey from Cli.Program
            IEnumerable<Token> tokenization = Enumerable.Empty<Token>();

            for (int x = 0; x == 0 || x < args.Count; ++x) {
                tokenization = tokenization.Concat(Tokenize.ProgramFile(File.ReadAllText(args[x]), args[x]));
            }

            var intermediateProgram = Parse.TangentProgram(tokenization);
            if (!intermediateProgram.Success) {
                Assert.Fail(string.Format("Errors during compilation: {0}", intermediateProgram.Error));
            }

            //var compiler = new CilCompiler();
            //compiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First()));

            NewCilCompiler.Compile(intermediateProgram.Result, Path.GetFileNameWithoutExtension(paths.First()));

            var discard = TimeSpan.Zero;
            return RunCompiledProgram(targetExe, out discard);
        }

        public static string DebugProgramFile(string path)
        {
            return DebugProgramFile(new[] { path });
        }

        public static string DebugProgramFile(IEnumerable<string> path, out TimeSpan compileDuration, out TimeSpan programDuration)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path);
        }

        public static string DebugProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration)
        {
            compileDuration = TimeSpan.Zero;
            programDuration = TimeSpan.Zero;
            return DebugProgramFile(path);
        }

        public static string ProgramFile(string path)
        {
            var discard = TimeSpan.Zero;
            return ProgramFile(path, out discard, out discard);
        }

        public static string ProgramFile(IEnumerable<string> paths)
        {
            TimeSpan discard;
            return ProgramFile(paths, out discard, out discard);
        }

        public static string ProgramFile(string path, out TimeSpan compileDuration, out TimeSpan programDuration)
        {
            return ProgramFile(new[] { path }, out compileDuration, out programDuration);
        }

        public static string ProgramFile(IEnumerable<string> paths, out TimeSpan compileDuration, out TimeSpan programDuration)
        {
            var compileProcess = new Process();
            var targetExe = Path.GetFileNameWithoutExtension(paths.First()) + ".exe";

            compileProcess.StartInfo.UseShellExecute = false;
            compileProcess.StartInfo.CreateNoWindow = true;
            compileProcess.StartInfo.RedirectStandardError = true;
            compileProcess.StartInfo.Arguments = string.Join(" ", paths.Concat(new[] { targetExe }));
            compileProcess.StartInfo.FileName = "Tangent.Cli.exe";

            var compileTimer = Stopwatch.StartNew();
            if (!compileProcess.Start()) {
                Assert.Fail("Process could not start.");
            }

            if (!compileProcess.WaitForExit(5000)) {
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
