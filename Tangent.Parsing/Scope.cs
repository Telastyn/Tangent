using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing {
    public class Scope {
        public readonly IEnumerable<ParameterDeclaration> Parameters;
        public readonly IEnumerable<TypeDeclaration> Types;
        public readonly IEnumerable<TypeResolvedReductionDeclaration> Functions;

        public Scope(IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> parameters, IEnumerable<TypeResolvedReductionDeclaration> functions) {
            Parameters = parameters.OrderByDescending(p => p.TakeParts().Count()).ToList();
            Types = types.OrderByDescending(t => t.TakeParts().Count()).ToList();
            Functions = functions.OrderByDescending(f => f.TakeParts().Count()).ToList();
        }
    }
}
