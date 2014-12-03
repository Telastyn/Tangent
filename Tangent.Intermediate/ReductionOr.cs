using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionOr<TRule, TResult> where TRule : class {
        public readonly TRule Rule;
        public readonly TResult Result;
        public bool IsReductionRule { get { return Rule != null; } }
        public bool IsEndResult { get { return !IsReductionRule; } }

        public ReductionOr(TRule rule) {
            Rule = rule;
        }

        public ReductionOr(TResult result) {
            Result = result;
        }
    }
}
