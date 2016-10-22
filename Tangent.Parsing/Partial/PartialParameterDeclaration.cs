using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialParameterDeclaration : ReductionRule<PartialPhrasePart, List<Expression>>
    {
        public readonly bool IsTypeParameter;

        public PartialParameterDeclaration(IdentifierExpression takes, List<Expression> returns, bool typeParam = false) : this(new[] { takes }, returns, typeParam) { }
        public PartialParameterDeclaration(IEnumerable<IdentifierExpression> takes, List<Expression> returns, bool typeParam = false) : base(takes.Select(id => new PartialPhrasePart(id)), returns)
        {
            IsTypeParameter = typeParam;
        }

        public PartialParameterDeclaration(IEnumerable<PartialPhrasePart> takes, List<Expression> returns, bool typeParam = false) : base(takes, returns)
        {
            IsTypeParameter = typeParam;
        }

        public override string SeparatorToken
        {
            get {
                if (IsTypeParameter) {
                    return "::";
                }

                return ":";
            }
        }

        public bool IsThisParam
        {
            get
            {
                return !IsTypeParameter && this.Takes.Count == 1 && this.Takes.First().IsIdentifier && this.Takes.First().Identifier.Identifier.Value == "this" && this.Returns.Count == 1 && this.Returns.First() is IdentifierExpression && ((IdentifierExpression)this.Returns.First()).Identifier.Value == "this";
            }
        }
    }
}
