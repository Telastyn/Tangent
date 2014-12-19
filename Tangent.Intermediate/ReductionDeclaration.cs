using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ReductionDeclaration : ReductionRule<PhrasePart, Function>
    {
        public ReductionDeclaration(Identifier takes, Function returns) : this(new[] { new PhrasePart(takes) }, returns) { }
        public ReductionDeclaration(PhrasePart takes, Function returns) : this(new[] { takes }, returns) { }
        public ReductionDeclaration(IEnumerable<PhrasePart> takes, Function returns) : base(takes, returns) { }
    }
}
