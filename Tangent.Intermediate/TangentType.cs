using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public enum KindOfType
    {
        Void,
        Enum,
        SingleValue
    }

    public class TangentType
    {
        public readonly KindOfType ImplementationType;

        protected TangentType(KindOfType impl)
        {
            ImplementationType = impl;
        }

        public static readonly TangentType Void = new TangentType(KindOfType.Void);
    }
}
