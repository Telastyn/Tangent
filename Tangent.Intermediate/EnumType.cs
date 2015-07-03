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
            return cache.GetOrAdd(id, i => new SingleValueType(this, i, NumericEquivalenceOf(id)));
        }

        private int NumericEquivalenceOf(Identifier id)
        {
            int ix = 1;
            foreach (var value in Values)
            {
                if (value == id)
                {
                    return ix;
                }

                ix++;
            }

            throw new InvalidOperationException();
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }
    }
}
