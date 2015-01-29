using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Tokenization
{
    public static class Tokenize
    {
        public static IEnumerable<Token> ProgramFile(string input)
        {
            int ix = 0;

            while (ix < input.Length) {
                ix = Skip(input, ix);
                if (ix == input.Length) {
                    yield break;
                }

                var token = Match("=>", TokenIdentifier.ReductionDeclSeparator, input, ix) ??
                    Match(":>", TokenIdentifier.TypeDeclSeparator, input, ix) ??
                    Identifier(input, ix) ??
                    Symbol(input, ix);

                yield return token;

                // For now, token cannot mismatch.
                ix = token.EndIndex;
            }
        }

        public static int Skip(string input, int index)
        {
            if (index >= input.Length) { return input.Length; }

            if (index < input.Length - 1) {

                // Comments.
                if (input[index] == '/' && input[index + 1] == '/') {
                    var eol = input.IndexOf('\n', index + 2);
                    if (eol == -1) { eol = input.Length - 1; }
                    return eol + 1;
                }
            }

            while (index < input.Length && char.IsWhiteSpace(input[index])) {
                index++;
            }

            return index;
        }

        public static Token Identifier(string input, int index)
        {
            if (index >= input.Length) { return null; }

            int endIx = index;

            // For now, let's just worry about ascii.
            while (endIx < input.Length && ((input[endIx] >= 'a' && input[endIx] <= 'z') || (input[endIx] >= 'A' && input[endIx] <= 'Z'))) {
                endIx++;
            }

            if (endIx == index) {
                return null;
            }

            return new Token(TokenIdentifier.Identifier, input, index, endIx);
        }

        public static Token Symbol(string input, int index)
        {
            if (index >= input.Length) { return null; }

            // For now, everything is cool.
            return new Token(TokenIdentifier.Symbol, input, index, index + 1);
        }

        private static Token Match(string target, TokenIdentifier id, string input, int index)
        {
            if (index > input.Length - target.Length) {
                return null;
            }

            if (input.Substring(index, target.Length) == target) {
                return new Token(id, input, index, index + target.Length);
            }

            return null;
        }
    }
}
