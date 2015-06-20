using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialTypeDeclaration : ReductionRule<PartialPhrasePart, TangentType>
    {
        public PartialTypeDeclaration(IEnumerable<PartialPhrasePart> takes, TangentType returns) : base(takes, returns) { }

        public override string SeparatorToken
        {
            get { return ":>"; }
        }
    }
}
