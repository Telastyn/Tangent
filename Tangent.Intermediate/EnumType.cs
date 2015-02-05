using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class EnumType : TangentType
    {
        public readonly IEnumerable<Identifier> Values;

        public EnumType(IEnumerable<Identifier> values)
            : base(KindOfType.Enum)
        {
            Values = values;
        }

        private ConcurrentDictionary<Identifier, SingleValueType> cache = new ConcurrentDictionary<Identifier, SingleValueType>();

        public SingleValueType SingleValueTypeFor(Identifier id)
        {
            if (!Values.Contains(id))
            {
                throw new InvalidOperationException();
            }

            return cache.GetOrAdd(id, i => new SingleValueType(this, i));
        }
    }
}
