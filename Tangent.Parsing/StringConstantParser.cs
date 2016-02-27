using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Parsing.Partial;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class StringConstantParser:Parser<ConstantElement<string>>
    {
        public override ResultOrParseError<ConstantElement<string>> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Identifier != TokenIdentifier.StringConstant) {
                consumed = 0;
                return new ResultOrParseError<ConstantElement<string>>(new ExpectedTokenParseError(TokenIdentifier.StringConstant, first));
            }

            consumed = 1;
            return new ConstantElement<string>(new ConstantExpression<string>(TangentType.String, first.Value, first.SourceInfo));
        }
    }
}
