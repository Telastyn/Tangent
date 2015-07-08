using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericArgumentReferenceType : TangentType
    {
        public readonly ParameterDeclaration GenericParameter;
        private GenericArgumentReferenceType(ParameterDeclaration genericParam)
            : base(KindOfType.GenericReference)
        {
            GenericParameter = genericParam;
        }

        private static ConcurrentDictionary<ParameterDeclaration, GenericArgumentReferenceType> cache = new ConcurrentDictionary<ParameterDeclaration, GenericArgumentReferenceType>();

        public static GenericArgumentReferenceType For(ParameterDeclaration decl)
        {
            return cache.GetOrAdd(decl, pd => new GenericArgumentReferenceType(pd));
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return mapping(this.GenericParameter);
        }

        public override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie)
        {
            if (tie == GenericTie.Reference) { yield return this.GenericParameter; }
        }
    }
}
