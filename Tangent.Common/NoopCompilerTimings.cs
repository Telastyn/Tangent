using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tangent
{
    public class NoopCompilerTimings : ICompilerTimings
    {
        public IEnumerable<CompilerTimingDatapoint> Timings
        {
            get
            {
                return Enumerable.Empty<CompilerTimingDatapoint>();
            }
        }

        public IDisposable Stopwatch(string module, string label = null, int? inputSize = default(int?), int? rulesetSize = default(int?), [CallerMemberName] string function = null)
        {
            return NoopDisposable.Common;
        }

        public string ToCSV(bool includeHeaders = false, bool durationInMilliseconds = false)
        {
            var result = new StringBuilder();
            if (includeHeaders) {
                result.AppendLine("Module, Function, Duration, Label, Input Size, Ruleset Size");
            }

            return result.ToString();
        }

        public class NoopDisposable : IDisposable
        {
            public static NoopDisposable Common = new NoopDisposable();
            public void Dispose()
            {
            }
        }
    }
}
