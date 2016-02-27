using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public class LiteralParser : Parser<bool>
    {
        public readonly TokenIdentifier Target;

        public LiteralParser(TokenIdentifier target)
        {
            Target = target;
        }

        public override ResultOrParseError<bool> Parse(IEnumerable<Token> tokens, out int consumed)
        {
            var first = tokens.FirstOrDefault();
            if (first == null || first.Identifier != Target) {
                consumed = 0;
                return new ResultOrParseError<bool>(new ExpectedTokenParseError(Target, first));
            }

            consumed = 1;
            return true;
        }

        public static readonly LiteralParser TypeArrow = new LiteralParser(TokenIdentifier.TypeArrow);
        public static readonly LiteralParser FunctionArrow = new LiteralParser(TokenIdentifier.FunctionArrow);
        public static readonly LiteralParser InterfaceBindingOperator = new LiteralParser(TokenIdentifier.InterfaceBindingOperator);
        public static readonly LiteralParser Colon = new LiteralParser(TokenIdentifier.Colon);
        public static readonly LiteralParser SemiColon = new LiteralParser(TokenIdentifier.SemiColon);
        public static readonly LiteralParser LazyOperator = new LiteralParser(TokenIdentifier.LazyOperator);
        public static readonly LiteralParser OpenParen = new LiteralParser(TokenIdentifier.OpenParen);
        public static readonly LiteralParser CloseParen = new LiteralParser(TokenIdentifier.CloseParen);
        public static readonly LiteralParser OpenCurly = new LiteralParser(TokenIdentifier.OpenCurly);
        public static readonly LiteralParser CloseCurly = new LiteralParser(TokenIdentifier.CloseCurly);

        public static implicit operator LiteralParser(string target)
        {
            switch (target) {
                case ":>": return TypeArrow;
                case "=>": return FunctionArrow;
                case ":<": return InterfaceBindingOperator;
                case ":": return Colon;
                case ";": return SemiColon;
                case "~>": return LazyOperator;
                case "(": return OpenParen;
                case ")": return CloseParen;
                case "{": return OpenCurly;
                case "}": return CloseCurly;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
