using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LazyType : TangentType
    {
        public readonly TangentType Type;
        public LazyType(TangentType t)
            : base(KindOfType.Lazy)
        {
            Type = t;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) { return true; }
            var lazyOther = other as LazyType;
            if (lazyOther == null) { return false; }
            return this.Type.CompatibilityMatches(lazyOther.Type, necessaryTypeInferences);
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this.Type.ResolveGenericReferences(mapping).Lazy;
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            foreach (var entry in Type.ContainedGenericReferences(tie)) {
                yield return entry;
            }
        }
    }
}
