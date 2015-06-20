using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class TypeDeclaration : ReductionRule<PhrasePart, TangentType>
    {
        public TypeDeclaration(PhrasePart takes, TangentType returns) : this(new[] { takes }, returns) { }
        public TypeDeclaration(Identifier takes, TangentType returns) : this(new PhrasePart(takes), returns) { }
        public TypeDeclaration(IEnumerable<PhrasePart> takes, TangentType returns) : base(takes, returns) { }
        public TypeDeclaration(IEnumerable<Identifier> takes, TangentType returns) : this(takes.Select(id => new PhrasePart(id)), returns) { }

        public bool IsGeneric
        {
            get
            {
                return !Takes.All(pp => pp.IsIdentifier);
            }
        }

        public override string SeparatorToken
        {
            get { return ":>"; }
        }
    }
}
