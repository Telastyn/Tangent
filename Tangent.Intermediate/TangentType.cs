using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class TangentType {
        public readonly IEnumerable<Identifier> Values;

        public TangentType(IEnumerable<Identifier> values) {
            Values = values;
        }

        public static readonly TangentType Void = new TangentType(Enumerable.Empty<Identifier>());
    }
}
