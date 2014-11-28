using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Tokenization;

namespace Tangent.Parsing {
    public static class Parse {
        public static ResultOrParseError<TangentProgram> TangentProgram(IEnumerable<Token> tokens) {
            return TangentProgram(new List<Token>(tokens));
        }

        private static ResultOrParseError<TangentProgram> TangentProgram(List<Token> tokens) {
            if (!tokens.Any()) {
                return new TangentProgram(Enumerable.Empty<TypeDeclaration>(), Enumerable.Empty<ReductionDeclaration>());
            }

            List<TypeDeclaration> types = new List<TypeDeclaration>();
            List<Tuple<Identifier, IEnumerable<Identifier>, PartialFunctionParse>> partialFunctionParses = new List<Tuple<Identifier, IEnumerable<Identifier>, PartialFunctionParse>>();

            while (tokens.Any()) {
                var first = tokens.First();
                tokens.RemoveAt(0);

                if (first.Identifier != TokenIdentifier.Identifier) {
                    return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                }

                var phraseBit = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => (Identifier)t.Value).ToList();
                tokens.RemoveRange(0, phraseBit.Count());
                if (!tokens.Any()) {
                    return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.TypeDeclSeparator, null));
                }

                var separator = tokens.First();
                tokens.RemoveAt(0);
                switch (separator.Identifier) {
                    case TokenIdentifier.TypeDeclSeparator:
                        var typeDecl = Type(tokens);
                        if (!typeDecl.Success) {
                            return new ResultOrParseError<TangentProgram>(typeDecl.Error);
                        }

                        types.Add(BuildTypeDeclaration(first.Value, phraseBit, typeDecl.Result));
                        break;
                    case TokenIdentifier.ReductionDeclSeparator:
                        var functionParts = PartialParseFunctionBits(tokens);
                        if (!functionParts.Success) {
                            return new ResultOrParseError<TangentProgram>(functionParts.Error);
                        }

                        partialFunctionParses.Add(new Tuple<Identifier, IEnumerable<Identifier>, PartialFunctionParse>(first.Value, phraseBit, functionParts.Result));
                        break;
                    default:
                        return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator));
                }
            }

            // Done. Move to Phase 2.
            throw new NotImplementedException();
        }

        public static ResultOrParseError<TangentType> Type(List<Token> tokens) {
            // For now, only enums.
            return Enum(tokens);
        }

        public static ResultOrParseError<TangentType> Enum(List<Token> tokens) {
            if (!MatchAndDiscard(TokenIdentifier.Identifier, "enum", tokens)) {
                return new ResultOrParseError<TangentType>(new ExpectedLiteralParseError("enum", tokens.FirstOrDefault()));
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                return new ResultOrParseError<TangentType>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            List<Identifier> result = new List<Identifier>();

            if (tokens.First().Identifier == TokenIdentifier.Identifier) {
                result.Add(tokens.First().Value);
                tokens.RemoveAt(0);
                while (MatchAndDiscard(TokenIdentifier.Symbol, ",", tokens)) {
                    var id = tokens.FirstOrDefault();
                    if (id == null || id.Identifier != TokenIdentifier.Identifier) {
                        return new ResultOrParseError<TangentType>(new ExpectedTokenParseError(TokenIdentifier.Identifier, id));
                    }

                    result.Add(id.Value);
                }
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens)) {
                return new ResultOrParseError<TangentType>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return new TangentType(result);
        }

        private static TypeDeclaration BuildTypeDeclaration(Identifier first, IEnumerable<Identifier> phraseBit, TangentType type) {
            dynamic last = type;
            foreach (var id in phraseBit.Reverse()) {
                last = new TypeDeclaration(id, last);
            }

            return new TypeDeclaration(first, last);
        }

        internal static ResultOrParseError<PartialFunctionParse> PartialParseFunctionBits(List<Token> tokens) {
            if (!tokens.Any()) {
                return new ResultOrParseError<PartialFunctionParse>(new ExpectedTokenParseError(TokenIdentifier.Identifier, null));
            }

            var identifiers = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new Identifier(t.Value)).ToList();
            if (!identifiers.Any()) {
                return new ResultOrParseError<PartialFunctionParse>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.First()));
            }

            tokens.RemoveRange(0, identifiers.Count());

            var partialBlock = PartialBlock(tokens);
            if (!partialBlock.Success) {
                return new ResultOrParseError<PartialFunctionParse>(partialBlock.Error);
            }

            return new PartialFunctionParse(identifiers, partialBlock.Result);
        }

        internal static ResultOrParseError<IEnumerable<IEnumerable<Identifier>>> PartialBlock(List<Token> tokens) {
            List<IEnumerable<Identifier>> result = new List<IEnumerable<Identifier>>();

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                return new ResultOrParseError<IEnumerable<IEnumerable<Identifier>>>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            while (tokens.Any() && tokens.First().Value != "}") {
                var statement = PartialStatement(tokens);
                if (!statement.Success) {
                    return new ResultOrParseError<IEnumerable<IEnumerable<Identifier>>>(statement.Error);
                }

                result.Add(statement.Result);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens)) {
                return new ResultOrParseError<IEnumerable<IEnumerable<Identifier>>>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return result;
        }

        internal static ResultOrParseError<IEnumerable<Identifier>> PartialStatement(List<Token> tokens) {
            var identifiers = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => (Identifier)t.Value).ToList();
            if (!identifiers.Any()) {
                return new ResultOrParseError<IEnumerable<Identifier>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
            }

            tokens.RemoveRange(0, identifiers.Count());

            if (!MatchAndDiscard(TokenIdentifier.Symbol, ";", tokens)) {
                return new ResultOrParseError<IEnumerable<Identifier>>(new ExpectedLiteralParseError(";", tokens.FirstOrDefault()));
            }

            return new ResultOrParseError<IEnumerable<Identifier>>(identifiers);
        }

        private static bool MatchAndDiscard(TokenIdentifier id, string value, List<Token> tokens) {
            if (!tokens.Any() || tokens.First().Identifier != id || tokens.First().Value != value) {
                return false;
            }

            tokens.RemoveAt(0);
            return true;
        }
    }
}
