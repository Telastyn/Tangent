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
    class Program
    {
        public static readonly string Usage = "Tangent.Cli.Exe <SourceFile> [DestFile]";
        
        static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            if(args.Length == 0){
                Console.Error.WriteLine(Usage);
            }

            var dest = "";

            if(args.Length == 1){
                dest = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
            }else{
                dest = args[1];
            }

            var tokenization = Tokenize.ProgramFile(File.ReadAllText(args[0]), args[0]);
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
