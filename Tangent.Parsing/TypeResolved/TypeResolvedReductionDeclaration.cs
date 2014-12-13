using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.TypeResolved {
    public class TypeResolvedReductionDeclaration: ReductionRule<PhrasePart, TypeResolvedFunction> {
        public TypeResolvedReductionDeclaration(Identifier takes, TypeResolvedFunction returns) : this(new[] { new PhrasePart(takes) }, returns) { }
        public TypeResolvedReductionDeclaration(PhrasePart takes, TypeResolvedFunction returns) : this(new[] { takes }, returns) { }
        public TypeResolvedReductionDeclaration(IEnumerable<PhrasePart> takes, TypeResolvedFunction returns) : base(takes, returns) { }
    }
}
