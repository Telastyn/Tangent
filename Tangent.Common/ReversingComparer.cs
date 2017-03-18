using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public class ReversingComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> wrapped;
        public ReversingComparer(IComparer<T> wrapped)
        {
            this.wrapped = wrapped;
        }

        public int Compare(T x, T y)
        {
            return wrapped.Compare(y, x);
        }
    }

    public static class ExtendIComparer
    {
        public static IComparer<T> Reverse<T>(this IComparer<T> comparer)
        {
            return new ReversingComparer<T>(comparer);
        }
    }
}
