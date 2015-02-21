using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public enum KindOfType
    {
        Builtin,
        Enum,
        SingleValue,
        Lazy
    }

    public class TangentType
    {
        public readonly KindOfType ImplementationType;

        protected TangentType(KindOfType impl)
        {
            ImplementationType = impl;
        }

        private LazyType lazy = null;

        public LazyType Lazy
        {
            get
            {
                lazy = lazy ?? new LazyType(this);
                return lazy;
            }
        }

        public static readonly TangentType Void = new TangentType(KindOfType.Builtin);
        public static readonly TangentType String = new TangentType(KindOfType.Builtin);
        public static readonly TangentType PotentiallyAnything = new TangentType(KindOfType.Builtin);
    }
}
