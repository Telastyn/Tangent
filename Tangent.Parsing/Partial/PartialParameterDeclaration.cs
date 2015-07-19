using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialParameterDeclaration : ReductionRule<Identifier, List<Expression>>
    {
        public PartialParameterDeclaration(Identifier takes, List<Expression> returns) : this(new[] { takes }, returns) { }
        public PartialParameterDeclaration(IEnumerable<Identifier> takes, List<Expression> returns) : base(takes, returns) { }

        public override string SeparatorToken
        {
            get { return ":"; }
        }

        public bool IsThisParam
        {
            get
            {
                return this.Takes.Count == 1 && this.Takes.First().Value == "this" && this.Returns.Count == 1 && this.Returns.First() is IdentifierExpression && ((IdentifierExpression)this.Returns.First()).Identifier.Value == "this";
            }
        }
    }
}
