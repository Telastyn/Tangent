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
        Product,
        Sum,
        Kind,
        TypeConstant,
        GenericReference,
        BoundGeneric,
        BoundGenericProduct,
        InferencePoint,
        Placeholder,
        Delegate
    }

    public enum GenericTie{
        Reference,
        Inference
    }

    public class TangentType
    {
        public readonly Guid Tracer = Guid.NewGuid();
        public readonly KindOfType ImplementationType;

        protected TangentType(KindOfType impl)
        {
            ImplementationType = impl;
        }

        public DelegateType Lazy
        {
            get
            {
                return DelegateType.For(Enumerable.Empty<TangentType>(), this);
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

        public virtual TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            return ContainedGenericReferences(tie, new HashSet<TangentType>());
        }

        protected internal virtual IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            yield break;
        }

        public static readonly TangentType Void = new TangentType(KindOfType.Builtin);
        public static readonly TangentType String = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Int = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Double = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Bool = new TangentType(KindOfType.Builtin);
        public static readonly TangentType Any = new TangentType(KindOfType.Builtin);
        public static readonly TangentType PotentiallyAnything = new TangentType(KindOfType.Builtin);
        public static readonly TangentType DontCare = new TangentType(KindOfType.Builtin);

        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), Tracer);
        }
    }
}
