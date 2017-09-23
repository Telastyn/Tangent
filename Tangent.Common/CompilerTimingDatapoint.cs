using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent
{
    public class CompilerTimingDatapoint
    {
        public readonly TimeSpan Duration;
        public readonly string Module;
        public readonly string Function;
        public readonly string Label;
        public readonly int? InputSize;
        public readonly int? RulesetSize;

        public CompilerTimingDatapoint(string module, string function, TimeSpan duration, string label = null, int? inputSize = null, int? rulesetSize = null)
        {
            this.Duration = duration;
            this.Module = module ?? "";
            this.Label = label ?? "";
            this.Function = function ?? "";
            this.InputSize = inputSize;
            this.RulesetSize = rulesetSize;
        }
    }
}
