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
        Lazy,
        Product,
        Sum,
        Kind,
        TypeConstant,
        GenericReference,
        BoundGeneric,
        InferencePoint,
        Placeholder
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

        private KindType kind = null;

        public KindType Kind
        {
            get
            {
                kind = kind ?? new KindType(this);
                return kind;
            }
        }

        private TypeConstant constant = null;
        public TypeConstant TypeConstant
        {
            get
            {
                constant = constant ?? new TypeConstant(this);
                return constant;
            }
        }

        public virtual bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public static readonly TangentType Void = new TangentType(KindOfType.Builtin);
        public static readonly TangentType String = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Int = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Double = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Bool = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Any = new TangentType(KindOfType.Builtin);
        public static readonly TangentType PotentiallyAnything = new TangentType(KindOfType.Builtin);
    }
}
