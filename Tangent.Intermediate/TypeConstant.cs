using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TypeConstant : TangentType
    {
        public readonly TangentType Value;
        public TypeConstant(TangentType value)
            : base(KindOfType.TypeConstant)
        {
            Value = value;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return Value.ResolveGenericReferences(mapping).TypeConstant;
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            foreach (var entry in Value.ContainedGenericReferences(tie)) {
                yield return entry;
            }
        }
    }
}
