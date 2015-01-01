using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ReductionRule<T, R>
    {
        public List<T> Takes { get; private set; }
        public R Returns { get; internal set; }

        public ReductionRule(IEnumerable<T> takes, R returns)
        {
            Takes = new List<T>(takes);
            Returns = returns;
        }
    }
}
