using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class SumType : TangentType
    {
        private readonly HashSet<TangentType> types;

        public IEnumerable<TangentType> Types
        {
            get
            {
                return types;
            }
        }

        private SumType(HashSet<TangentType> types):base(KindOfType.Sum)
        {
            this.types = types;
        }

        private static readonly Dictionary<int, List<SumType>> creationCache = new Dictionary<int, List<SumType>>();

        public static SumType For(IEnumerable<TangentType> types)
        {
            HashSet<TangentType> set = new HashSet<TangentType>(types);
            List<SumType> existing;
            bool newb = false;
            if (creationCache.TryGetValue(set.Count, out existing)) {
                foreach (var entry in existing) {
                    if (entry.Types.SequenceEqual(set)) {
                        return entry;
                    }
                }
            } else {
                newb = true;
            }

            var result = new SumType(set);
            if (newb) {
                creationCache.Add(set.Count, new List<SumType>() { result });
            } else {
                creationCache[set.Count].Add(result);
            }

            return result;
        }
    }
}
