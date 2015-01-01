using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ParameterDeclaration : ReductionRule<Identifier, TangentType>
    {
        public ParameterDeclaration(Identifier takes, TangentType returns) : this(new[] { takes }, returns) { }
        public ParameterDeclaration(IEnumerable<Identifier> takes, TangentType returns) : base(takes, returns) { }
    }
}
