using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.TypeResolved {
    public class TypeResolvedReductionDeclaration: ReductionRule<PhrasePart, TypeResolvedFunction> {
        public TypeResolvedReductionDeclaration(PhrasePart takes, ReductionRule<PhrasePart, TypeResolvedFunction> reduction) : base(takes, reduction) { }
        public TypeResolvedReductionDeclaration(PhrasePart takes, TypeResolvedFunction returns) : base(takes, returns) { }
        public TypeResolvedReductionDeclaration(IEnumerable<PhrasePart> takes, TypeResolvedFunction returns) : base(takes, returns) { }
    }
}
