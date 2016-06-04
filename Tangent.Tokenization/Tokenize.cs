using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Tokenization
{
    public static class Tokenize
    {
        public static IEnumerable<Token> ProgramFile(string input, string inputLabel)
        {
            int ix = 0;

            while (ix < input.Length) {
                ix = Skip(input, ix);
                if (ix == input.Length) {
                    yield break;
                }

                var token = Match("=>", TokenIdentifier.FunctionArrow, input, ix, inputLabel) ??
                    Match(":>", TokenIdentifier.TypeArrow, input, ix, inputLabel) ??
                    Match(":=", TokenIdentifier.InitializerEquals, input, ix, inputLabel) ??
                    Match("~>", TokenIdentifier.LazyOperator, input, ix, inputLabel) ??
                    Match(":<", TokenIdentifier.InterfaceBindingOperator, input, ix, inputLabel) ??
                    Match(":", TokenIdentifier.Colon, input, ix, inputLabel) ??
                    Match("(", TokenIdentifier.OpenParen, input, ix, inputLabel) ??
                    Match(")", TokenIdentifier.CloseParen, input, ix, inputLabel) ??
                    Match("{", TokenIdentifier.OpenCurly, input, ix, inputLabel) ??
                    Match("}", TokenIdentifier.CloseCurly, input, ix, inputLabel) ??
                    Match(";", TokenIdentifier.SemiColon, input, ix, inputLabel) ??
                    Identifier(inputLabel, input, ix) ??
                    String(inputLabel, input, ix) ??
                    IntegerConstant(inputLabel, input, ix) ??
                    Symbol(inputLabel, input, ix);

                yield return token;

                // For now, token cannot mismatch.
                ix = token.EndIndex;
            }
        }

        public static int Skip(string input, int index)
        {
            if (index >= input.Length) { return input.Length; }

            bool go = false;
            do {
                go = false;

                if (index < input.Length - 1) {

                    // Comments.
                    if (input[index] == '/' && input[index + 1] == '/') {
                        var eol = input.IndexOf('\n', index + 2);
                        if (eol == -1) { eol = input.Length - 1; }
                        index = eol + 1;
                        go = true;
                    }
                }

                while (index < input.Length && char.IsWhiteSpace(input[index])) {
                    index++;
                    go = true;
                }
            } while (go);

            return index;
        }

        public static Token Identifier(string inputLabel, string input, int index)
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

            return new Token(TokenIdentifier.Identifier, input, index, endIx, inputLabel);
        }

        public static Token String(string inputLabel, string input, int index)
        {
            // Starting with the basics.
            if (index >= input.Length) { return null; }
            if (input[index] == '\"') {
                int endIx;
                for (endIx = index + 1; endIx < input.Length && input[endIx] != '\"'; ++endIx) { }
                if (endIx == input.Length) {
                    // TODO: pleasant error.
                    return null;
                }

                return new Token(TokenIdentifier.StringConstant, input, index, endIx + 1, inputLabel);
            } else {
                return null;
            }
        }

        public static Token IntegerConstant(string inputLabel, string input, int index)
        {
            int ix = index;
            while (ix < input.Length && char.IsDigit(input[ix])) {
                ix++;
            }

            if (ix == index) { return null; }
            return new Token(TokenIdentifier.IntegerConstant, input, index, ix, inputLabel);
        }

        public static Token Symbol(string inputLabel, string input, int index)
        {
            if (index >= input.Length) { return null; }

            // For now, everything is cool.
            return new Token(TokenIdentifier.Identifier, input, index, index + 1, inputLabel);
        }

        private static Token Match(string target, TokenIdentifier id, string input, int index, string inputLabel)
        {
            if (index > input.Length - target.Length) {
                return null;
            }

            if (input.Substring(index, target.Length) == target) {
                return new Token(id, input, index, index + target.Length, inputLabel);
            }

            return null;
        }
    }
}
