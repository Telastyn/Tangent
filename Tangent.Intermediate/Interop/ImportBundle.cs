using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class ImportBundle
    {
        public readonly IEnumerable<TypeDeclaration> TypeDeclarations;
        public readonly IEnumerable<ReductionDeclaration> Functions;

        public static readonly ImportBundle Empty = new ImportBundle(Enumerable.Empty<TypeDeclaration>(), Enumerable.Empty<ReductionDeclaration>());

        public ImportBundle(IEnumerable<TypeDeclaration> types, IEnumerable<ReductionDeclaration> functions)
        {
            TypeDeclarations = new List<TypeDeclaration>(types);
            Functions = new List<ReductionDeclaration>(functions);
        }
    }
}
