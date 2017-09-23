using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tangent
{
    public class CompilerTimings : ICompilerTimings
    {
        private ConcurrentBag<CompilerTimingDatapoint> timings = new ConcurrentBag<CompilerTimingDatapoint>();

        public CompilerTimings() { }
        public CompilerTimings(IEnumerable<CompilerTimingDatapoint> datapoints)
        {
            timings = new ConcurrentBag<CompilerTimingDatapoint>(datapoints);
        }

        public static CompilerTimings operator+(CompilerTimings a, CompilerTimings b)
        {
            return new CompilerTimings(a.timings.Concat(b.timings));
        }

        public IEnumerable<CompilerTimingDatapoint> Timings { get { return timings; } }

        public IDisposable Stopwatch(string module, string label = null, int? inputSize = null, int? rulesetSize = null, [CallerMemberName]string function = null)
        {
            return new CompilerTimingScope(this, module, function, label, inputSize, rulesetSize);
        }

        public string ToCSV(bool includeHeaders = false, bool durationInMilliseconds = false)
        {
            var result = new StringBuilder();
            if (includeHeaders) {
                result.AppendLine("Module, Function, Duration, Label, Input Size, Ruleset Size");
            }

            foreach(var entry in timings) {
                var durationString = durationInMilliseconds ? entry.Duration.TotalMilliseconds.ToString() : entry.Duration.ToString();
                result.AppendLine($"{entry.Module}, {entry.Function}, {durationString}, {entry.Label}, {entry.InputSize}, {entry.RulesetSize}");
            }

            return result.ToString();
        }

        public class CompilerTimingScope : IDisposable
        {
            private Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            private bool disposedValue = false;
            private readonly CompilerTimings profiler;
            private readonly string module;
            private readonly string function;
            private readonly string label;
            private readonly int? inputSize;
            private readonly int? rulesetSize;

            internal CompilerTimingScope(CompilerTimings owningProfiler, string module, string function, string label, int? inputSize, int? rulesetSize)
            {
                this.profiler = owningProfiler;
                this.module = module;
                this.function = function;
                this.label = label;
                this.inputSize = inputSize;
                this.rulesetSize = rulesetSize;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue) {
                    if (disposing) {
                        profiler.timings.Add(new CompilerTimingDatapoint(module, function, timer.Elapsed, label, inputSize, rulesetSize));
                    }

                    disposedValue = true;
                }
            }


            public void Dispose()
            {
                Dispose(true);
            }
        }
    }
}
