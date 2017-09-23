using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tangent
{
    public interface ICompilerTimings
    {
        IEnumerable<CompilerTimingDatapoint> Timings { get; }

        IDisposable Stopwatch(string module, string label = null, int? inputSize = default(int?), int? rulesetSize = default(int?), [CallerMemberName] string function = null);
        string ToCSV(bool includeHeaders = false, bool durationInMilliseconds = false);
    }
}