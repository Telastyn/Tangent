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
    public class IntConstantParser : Parser<ConstantElement<int>>
    {
        public override ResultOrParseError<ConstantElement<int>> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Identifier != TokenIdentifier.IntegerConstant) {
                consumed = 0;
                return new ResultOrParseError<ConstantElement<int>>(new ExpectedTokenParseError(TokenIdentifier.IntegerConstant, first));
            }

            consumed = 1;
            return new ConstantElement<int>(new ConstantExpression<int>(TangentType.Int, int.Parse(first.Value), first.SourceInfo));
        }
    }
}
