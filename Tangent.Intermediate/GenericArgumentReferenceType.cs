using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericArgumentReferenceType:TangentType
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
    }
}
