using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
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
            List<PartialReductionDeclaration> partialFunctions = new List<PartialReductionDeclaration>();

            while (tokens.Any()) {
                var shouldBePhrase = PartialPhrase(tokens);
                if (!shouldBePhrase.Success) {
                    return new ResultOrParseError<TangentProgram>(shouldBePhrase.Error);
                }

                var phrase = shouldBePhrase.Result;

                var separator = tokens.First();
                tokens.RemoveAt(0);
                switch (separator.Identifier) {
                    case TokenIdentifier.TypeDeclSeparator:
                        var typeDecl = Type(tokens);
                        if (!typeDecl.Success) {
                            return new ResultOrParseError<TangentProgram>(typeDecl.Error);
                        }

                        if (!phrase.All(pp => pp.IsIdentifier)) {
                            return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator));
                        }

                        types.Add(new TypeDeclaration(phrase.Select(pp => pp.Identifier), typeDecl.Result));
                        break;
                    case TokenIdentifier.ReductionDeclSeparator:
                        var functionParts = PartialParseFunctionBits(tokens);
                        if (!functionParts.Success) {
                            return new ResultOrParseError<TangentProgram>(functionParts.Error);
                        }

                        partialFunctions.Add(new PartialReductionDeclaration(phrase, functionParts.Result));
                        break;
                    default:
                        return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator));
                }
            }

            // Done. Move to Phase 2.
            throw new NotImplementedException();
        }

        internal static ResultOrParseError<List<PartialPhrasePart>> PartialPhrase(List<Token> tokens) {
            var phrase = new List<PartialPhrasePart>();


            ResultOrParseError<PartialPhrasePart> part = null;
            while (true) {
                part = TryPartialPhrasePart(tokens);
                if (part == null) {
                    if (phrase.Any()) {
                        return phrase;
                    } else {
                        return new ResultOrParseError<List<PartialPhrasePart>>(new ExpectedLiteralParseError("phrase part", tokens.FirstOrDefault()));
                    }
                } else {
                    if (part.Success) {
                        phrase.Add(part.Result);
                    } else {
                        return new ResultOrParseError<List<PartialPhrasePart>>(part.Error);
                    }
                }
            }
        }

        internal static ResultOrParseError<PartialPhrasePart> TryPartialPhrasePart(List<Token> tokens) {
            var first = tokens.FirstOrDefault();
            if (first == null) {
                return null;
            }

            if (first.Identifier == TokenIdentifier.Identifier) {
                tokens.RemoveAt(0);
                return new PartialPhrasePart(first.Value);
            }

            if (first.Identifier == TokenIdentifier.Symbol && first.Value == "(") {
                tokens.RemoveAt(0);
                var paramName = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t=>new Identifier(t.Value)).ToList();
                tokens.RemoveRange(0, paramName.Count);
                if (paramName.Count == 0) {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
                }

                var colon = tokens.FirstOrDefault();
                if (colon == null || colon.Value != ":") {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedLiteralParseError(":", colon));
                }

                tokens.RemoveAt(0);

                var typeName = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new Identifier(t.Value)).ToList();
                tokens.RemoveRange(0, typeName.Count);
                if (typeName.Count == 0) {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
                }

                var close = tokens.FirstOrDefault();
                if (close == null || close.Value != ")") {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedLiteralParseError(")", close));
                }

                tokens.RemoveAt(0);

                return new PartialPhrasePart(new PartialParameterDeclaration(paramName, typeName));
            }

            return null;
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

        internal static ResultOrParseError<PartialFunction> PartialParseFunctionBits(List<Token> tokens) {
            if (!tokens.Any()) {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, null));
            }

            var identifiers = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new Identifier(t.Value)).ToList();
            if (!identifiers.Any()) {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.First()));
            }

            tokens.RemoveRange(0, identifiers.Count());

            var partialBlock = PartialBlock(tokens);
            if (!partialBlock.Success) {
                return new ResultOrParseError<PartialFunction>(partialBlock.Error);
            }

            return new PartialFunction(identifiers, partialBlock.Result);
        }

        internal static ResultOrParseError<PartialBlock> PartialBlock(List<Token> tokens) {
            List<IEnumerable<Identifier>> result = new List<IEnumerable<Identifier>>();

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            while (tokens.Any() && tokens.First().Value != "}") {
                var statement = PartialStatement(tokens);
                if (!statement.Success) {
                    return new ResultOrParseError<PartialBlock>(statement.Error);
                }

                result.Add(statement.Result);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens)) {
                return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return new ResultOrParseError<PartialBlock>(new PartialBlock(result.Select(stmt => new PartialStatement(stmt))));
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
