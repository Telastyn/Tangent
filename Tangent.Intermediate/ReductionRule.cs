using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionRule<T, R> {
        public readonly T Takes;
        public readonly ReductionOr<ReductionRule<T, R>, R> Returns;

        public ReductionRule(T takes, ReductionRule<T, R> reduction) {
            Takes = takes;
            Returns = new ReductionOr<ReductionRule<T, R>, R>(reduction);
        }

        public ReductionRule(T takes, R returns) {
            Takes = takes;
            Returns = new ReductionOr<ReductionRule<T, R>, R>(returns);
        }

        public ReductionRule(IEnumerable<T> takes, R returns) {
            if (!takes.Any()) {
                throw new InvalidOperationException("Must supply at least one parameter.");
            }

            dynamic r = returns;
            foreach (var t in takes.Reverse()) {
                r = new ReductionRule<T, R>(t, r);
            }

            Takes = r.Takes;
            Returns = r.Returns;
        }

        public IEnumerable<T> TakeParts() {
            yield return Takes;
            var rule = Returns.Rule;
            while (rule != null) {
                yield return rule.Takes;
                rule = rule.Returns.Rule;
            }
        }

        public R EndResult() {
            var r = Returns;
            while (!r.IsEndResult) {
                r = r.Rule.Returns;
            }

            return r.Result;
        }
    }
}
