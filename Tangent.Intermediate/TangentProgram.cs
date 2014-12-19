using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class TangentProgram
    {
        public readonly IEnumerable<TypeDeclaration> TypeDeclarations;
        public readonly IEnumerable<ReductionDeclaration> Functions;

        public TangentProgram(IEnumerable<TypeDeclaration> types, IEnumerable<ReductionDeclaration> functions)
        {
            this.TypeDeclarations = types;
            this.Functions = functions;
        }
    }
}
