using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T value)
        {
            var enumerator = collection.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); ++i) {
                if (enumerator.Current.Equals(value)) { return i; }
            }

            throw new InvalidOperationException("Specified value does not exist in collection.");
        }
    }
}
