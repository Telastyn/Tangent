using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Tangent.CilGeneration;
using Tangent.Parsing;
using Tangent.Tokenization;

namespace Tangent.Cli
{
    public class Program
    {
        public static readonly string Usage = @"Tangent.Cli.exe <SourceFile> [SourceFile2..N] [DestFile]
Tangent.Cli.exe -f <CompilerInputFile>";

        public static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            if (args.Length == 0) {
                Console.Error.WriteLine(Usage);
                Environment.Exit(1);
            }

            var inputs = new CompilerInputs1();

            if (args[0] == "-f") {
                if (args.Length == 1) {
                    Console.Error.WriteLine(Usage);
                    Environment.Exit(1);
                }

                inputs = JsonConvert.DeserializeObject<CompilerInputs1>(File.ReadAllText(args[1]));
                inputs.DestinationFile = Path.GetFileNameWithoutExtension(inputs.DestinationFile);
            } else {
                if (args.Length == 1) {
                    inputs.DestinationFile = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
                } else {
                    inputs.DestinationFile = args[args.Length - 1];
                }

                for (int x = 0; x == 0 || x < args.Length - 1; ++x) {
                    inputs.SourceFiles.Add(args[x]);
                }
            }

            IEnumerable<Token> tokenization = Enumerable.Empty<Token>();

            foreach (var sourceFile in inputs.SourceFiles) {
                tokenization = tokenization.Concat(Tokenize.ProgramFile(File.ReadAllText(sourceFile), sourceFile));
            }

            var intermediateProgram = Parse.TangentProgram(tokenization, Tangent.Intermediate.Interop.TangentImport.ImportAssemblies(inputs.DllImports.Select(f => Assembly.Load(f))));
            if (!intermediateProgram.Success) {
                Console.Error.WriteLine(intermediateProgram.Error); // TODO: make better.
                return;
            }

            NewCilCompiler.Compile(intermediateProgram.Result, inputs.DestinationFile);
            Debug.WriteLine("Compile Duration: " + timer.Elapsed);
        }
    }
}
