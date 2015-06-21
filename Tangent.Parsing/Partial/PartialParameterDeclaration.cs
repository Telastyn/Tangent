using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialParameterDeclaration : ReductionRule<Identifier, List<IdentifierExpression>>
    {
        public PartialParameterDeclaration(Identifier takes, List<IdentifierExpression> returns) : this(new[] { takes }, returns) { }
        public PartialParameterDeclaration(IEnumerable<Identifier> takes, List<IdentifierExpression> returns) : base(takes, returns) { }

        public override string SeparatorToken
        {
            get { return ":"; }
        }

        public bool IsThisParam
        {
            get
            {
                return this.Takes.Count == 1 && this.Takes.First().Value == "this" && this.Returns.Count == 1 && this.Returns.First().Identifier.Value == "this";
            }
        }
    }
}
