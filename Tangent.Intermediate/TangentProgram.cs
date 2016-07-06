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
        public readonly IEnumerable<string> InputLabels;
        public readonly IEnumerable<Field> Fields;

        public TangentProgram(IEnumerable<TypeDeclaration> types, IEnumerable<ReductionDeclaration> functions, IEnumerable<Field> globalFields, IEnumerable<string> inputLabels)
        {
            this.TypeDeclarations = types;
            this.Functions = functions;
            this.Fields = globalFields;
            this.InputLabels = inputLabels;
        }
    }
}
