using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Tangent.CilGeneration;
using Tangent.Parsing;
using Tangent.Tokenization;

namespace Tangent.Cli
{
    public class Program
    {
        public static readonly string Usage = "Tangent.Cli.Exe <SourceFile> [SourceFile2..N] [DestFile]";

        public static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            if (args.Length == 0) {
                Console.Error.WriteLine(Usage);
            }

            var dest = "";

            if (args.Length == 1) {
                dest = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
            } else {
                dest = args[args.Length - 1];
            }

            IEnumerable<Token> tokenization = Enumerable.Empty<Token>();

            for (int x = 0; x == 0 || x < args.Length - 1; ++x) {
                tokenization = tokenization.Concat(Tokenize.ProgramFile(File.ReadAllText(args[x]), args[x]));
            }

            var intermediateProgram = Parse.TangentProgram(tokenization);
            if (!intermediateProgram.Success) {
                Console.Error.WriteLine(intermediateProgram.Error); // TODO: make better.
                return;
            }

            var compiler = new CilCompiler();
            compiler.Compile(intermediateProgram.Result, dest);
            Debug.WriteLine("Compile Duration: " + timer.Elapsed);
        }
    }
}
