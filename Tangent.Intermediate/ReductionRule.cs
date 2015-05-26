using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public abstract class ReductionRule<T, R>
    {
        public List<T> Takes { get; private set; }
        public R Returns { get; internal set; }

        public ReductionRule(IEnumerable<T> takes, R returns)
        {
            Takes = new List<T>(takes);
            Returns = returns;
        }

        public abstract string SeparatorToken { get; }

        public override string ToString()
        {
            return string.Format("{0} {2} {1}", string.Join(" ", Takes), Returns, SeparatorToken);
        }
    }
}
