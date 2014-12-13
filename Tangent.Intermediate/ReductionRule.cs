using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionRule<T, R> {
        public readonly List<T> Takes;
        public readonly R Returns;

        public ReductionRule(IEnumerable<T> takes, R returns) {
            if (!takes.Any()) {
                throw new InvalidOperationException("ReductionRule must have some input.");
            }

            Takes = new List<T>(takes);
            Returns = returns;
        }
    }
}
