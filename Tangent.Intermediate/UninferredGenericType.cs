using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class UninferredGenericType : TangentType
    {
        public readonly GenericArgumentReferenceType GenericReference;

        private UninferredGenericType(GenericArgumentReferenceType generic) : base(KindOfType.UninferredGenericReference)
        {
            GenericReference = generic;
        }

        private static readonly ConcurrentDictionary<GenericArgumentReferenceType, UninferredGenericType> Cache = new ConcurrentDictionary<GenericArgumentReferenceType, UninferredGenericType>();

        public static UninferredGenericType For(GenericArgumentReferenceType generic)
        {
            return Cache.GetOrAdd(generic, a => new UninferredGenericType(generic));
        }

        public static UninferredGenericType For(ParameterDeclaration pd)
        {
            return For(GenericArgumentReferenceType.For(pd));
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if(other == this) { return true; }
            if (other == GenericReference) { return true; }
            var otherUninferred = other as UninferredGenericType;

            if(otherUninferred != null && otherUninferred.GenericReference == this.GenericReference) {
                return true;
            }

            return false;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return mapping(GenericReference.GenericParameter);
        }

        public override string ToString()
        {
            return string.Format("!{0}!", GenericReference.ToString());
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            return GenericReference.ContainedGenericReferences(alreadyProcessed);
        }
    }
}
