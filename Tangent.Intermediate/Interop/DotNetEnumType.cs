using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DotNetEnumType : EnumType
    {
        public readonly Type DotNetType;
        private readonly List<int> IntValues;

        private DotNetEnumType(Type dotNetType) : base(dotNetType.IsEnum ? Enum.GetNames(dotNetType).Select(s => new Identifier(s)) : Enumerable.Empty<Identifier>())
        {
            if (!dotNetType.IsEnum) {
                throw new InvalidOperationException("Specified type must be an enum.");
            }

            var underlyingEnumType = dotNetType.GetEnumUnderlyingType();
            if (underlyingEnumType == typeof(long)) {
                throw new NotImplementedException();
            }

            DotNetType = dotNetType;
            var arr = DotNetType.GetEnumValues();
            IntValues = new List<int>();
            foreach (var entry in arr) {
                IntValues.Add(Convert.ToInt32(entry));
            }
        }

        private static ConcurrentDictionary<Type, DotNetEnumType> cache = new ConcurrentDictionary<Type, DotNetEnumType>();

        public static DotNetEnumType For(Type dotNetType)
        {
            return cache.GetOrAdd(dotNetType, t => new DotNetEnumType(t));
        }

        protected override int NumericEquivalenceOf(Identifier id)
        {
            return IntValues[Values.IndexOf(id)];
        }
    }
}
