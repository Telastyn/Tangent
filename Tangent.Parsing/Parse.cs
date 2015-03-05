using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public static class Parse
    {
        public static ResultOrParseError<TangentProgram> TangentProgram(IEnumerable<Token> tokens)
        {
            return TangentProgram(new List<Token>(tokens));
        }

        private static ResultOrParseError<TangentProgram> TangentProgram(List<Token> tokens)
        {
            if (!tokens.Any())
            {
                return new TangentProgram(Enumerable.Empty<TypeDeclaration>(), Enumerable.Empty<ReductionDeclaration>());
            }

            List<TypeDeclaration> types = new List<TypeDeclaration>() { new TypeDeclaration("void", TangentType.Void) };
            List<PartialReductionDeclaration> partialFunctions = new List<PartialReductionDeclaration>();

            while (tokens.Any())
            {
                var shouldBePhrase = PartialPhrase(tokens);
                if (!shouldBePhrase.Success)
                {
                    return new ResultOrParseError<TangentProgram>(shouldBePhrase.Error);
                }

                var phrase = shouldBePhrase.Result;

                var separator = tokens.First();
                tokens.RemoveAt(0);
                switch (separator.Identifier)
                {
                    case TokenIdentifier.TypeDeclSeparator:
                        var typeDecl = Type(tokens);
                        if (!typeDecl.Success)
                        {
                            return new ResultOrParseError<TangentProgram>(typeDecl.Error);
                        }

                        if (!phrase.All(pp => pp.IsIdentifier))
                        {
                            return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator));
                        }

                        types.Add(new TypeDeclaration(phrase.Select(pp => pp.Identifier), typeDecl.Result));
                        break;
                    case TokenIdentifier.ReductionDeclSeparator:
                        var functionParts = PartialParseFunctionBits(tokens);
                        if (!functionParts.Success)
                        {
                            return new ResultOrParseError<TangentProgram>(functionParts.Error);
                        }

                        partialFunctions.Add(new PartialReductionDeclaration(phrase, functionParts.Result));
                        break;
                    default:
                        return new ResultOrParseError<TangentProgram>(new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator));
                }
            }

            // Move to Phase 2 - Resolve types in parameters and function return types.
            var resolvedFunctions = TypeResolve.AllPartialFunctionDeclarations(partialFunctions, types);
            if (!resolvedFunctions.Success)
            {
                return new ResultOrParseError<TangentProgram>(resolvedFunctions.Error);
            }

            // And now Phase 3 - Statement parsing based on syntax.
            var lookup = new Dictionary<Function, Function>();
            var bad = new List<IncomprehensibleStatementError>();
            var ambiguous = new List<AmbiguousStatementError>();

            foreach (var fn in resolvedFunctions.Result)
            {
                TypeResolvedFunction partialFunction = fn.Returns as TypeResolvedFunction;
                if (partialFunction != null)
                {
                    var scope = new Scope(partialFunction.EffectiveType, types, fn.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), resolvedFunctions.Result.Concat(BuiltinFunctions.All));
                    Function newb = BuildBlock(scope, partialFunction.EffectiveType, partialFunction.Implementation, bad, ambiguous);

                    lookup.Add(partialFunction, newb);
                }
                else
                {
                    throw new NotImplementedException("We shouldn't get here... No optimizations exist to fully resolve functions in one step.");
                }
            }

            if (bad.Any() || ambiguous.Any())
            {
                return new ResultOrParseError<TangentProgram>(new StatementGrokErrors(bad, ambiguous));
            }

            // 3a - Replace TypeResolvedFunctions with fully resolved ones.
            var workset = new HashSet<Expression>();
            foreach (var fn in lookup.Values)
            {
                fn.ReplaceTypeResolvedFunctions(lookup, workset);
            }

            return new TangentProgram(types, resolvedFunctions.Result.Select(fn =>
            {
                if (fn.Returns is TypeResolvedFunction)
                {
                    return new ReductionDeclaration(fn.Takes, lookup[fn.Returns]);
                }
                else
                {
                    return fn;
                }
            }).ToList());
        }

        private static Function BuildBlock(Scope scope, TangentType effectiveType, PartialBlock partialBlock, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            var block = BuildBlock(scope, effectiveType, partialBlock.Statements, bad, ambiguous);

            return new Function(effectiveType, block);
        }

        private static Block BuildBlock(Scope scope, TangentType effectiveType, IEnumerable<PartialStatement> elements, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            List<Expression> statements = new List<Expression>();
            if (!elements.Any())
            {
                if (effectiveType != TangentType.Void) { bad.Add(new IncomprehensibleStatementError(Enumerable.Empty<Expression>())); }
                return new Block(Enumerable.Empty<Expression>());
            }

            var allElements = elements.ToList();
            for (int ix = 0; ix < allElements.Count; ++ix)
            {
                var line = allElements[ix];
                var statementBits = line.FlatTokens.Select(t => ElementToExpression(scope, t, bad, ambiguous));
                var statement = new Input(statementBits, scope).InterpretTowards((effectiveType != null && ix == allElements.Count - 1) ? effectiveType : TangentType.Void);
                if (statement.Count == 0)
                {
                    bad.Add(new IncomprehensibleStatementError(statementBits));
                }
                else if (statement.Count > 1)
                {
                    ambiguous.Add(new AmbiguousStatementError(statementBits, statement));
                }
                else
                {
                    statements.Add(statement.First());
                }
            }

            return new Block(statements);
        }

        private static Expression ElementToExpression(Scope scope, PartialElement element, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            switch (element.Type)
            {
                case ElementType.Identifier:
                    return new IdentifierExpression(((IdentifierElement)element).Identifier);
                case ElementType.Parens:
                    throw new NotImplementedException("Parens expressions not yet supported.");
                case ElementType.Block:
                    var stmts = ((BlockElement)element).Block.Statements.ToList();
                    var last = stmts.Last();
                    stmts.RemoveAt(stmts.Count - 1);
                    return new ParenExpression(BuildBlock(scope, null, stmts, bad, ambiguous), last.FlatTokens.Select(e => ElementToExpression(scope, e, bad, ambiguous)).ToList());
                case ElementType.Constant:
                    return ((ConstantElement)element).TypelessExpression;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static ResultOrParseError<List<PartialPhrasePart>> PartialPhrase(List<Token> tokens)
        {
            var phrase = new List<PartialPhrasePart>();

            ResultOrParseError<PartialPhrasePart> part = null;
            while (true)
            {
                part = TryPartialPhrasePart(tokens);
                if (part == null)
                {
                    if (phrase.Any())
                    {
                        return phrase;
                    }
                    else
                    {
                        return new ResultOrParseError<List<PartialPhrasePart>>(new ExpectedLiteralParseError("phrase part", tokens.FirstOrDefault()));
                    }
                }
                else
                {
                    if (part.Success)
                    {
                        phrase.Add(part.Result);
                    }
                    else
                    {
                        return new ResultOrParseError<List<PartialPhrasePart>>(part.Error);
                    }
                }
            }
        }

        internal static ResultOrParseError<PartialPhrasePart> TryPartialPhrasePart(List<Token> tokens)
        {
            var first = tokens.FirstOrDefault();
            if (first == null)
            {
                return null;
            }

            if (first.Identifier == TokenIdentifier.Identifier)
            {
                tokens.RemoveAt(0);
                return new PartialPhrasePart(first.Value);
            }

            if (first.Identifier == TokenIdentifier.Symbol && first.Value == "(")
            {
                tokens.RemoveAt(0);
                var paramName = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new Identifier(t.Value)).ToList();
                tokens.RemoveRange(0, paramName.Count);
                if (paramName.Count == 0)
                {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
                }

                var colon = tokens.FirstOrDefault();
                if (colon == null || colon.Value != ":")
                {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedLiteralParseError(":", colon));
                }

                tokens.RemoveAt(0);

                var typeName = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier || t.Identifier == TokenIdentifier.LazyOperator || (t.Value == ".")).Select(t => new Identifier(t.Value)).ToList();
                tokens.RemoveRange(0, typeName.Count);
                if (typeName.Count == 0)
                {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
                }

                var close = tokens.FirstOrDefault();
                if (close == null || close.Value != ")")
                {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedLiteralParseError(")", close));
                }

                tokens.RemoveAt(0);

                return new PartialPhrasePart(new PartialParameterDeclaration(paramName, typeName));
            }

            return null;
        }

        public static ResultOrParseError<TangentType> Type(List<Token> tokens)
        {
            // For now, only enums.
            var enumResult = Enum(tokens);
            if (enumResult.Success)
            {
                return enumResult.Result;
            }
            else
            {
                return new ResultOrParseError<TangentType>(enumResult.Error);
            }
        }

        public static ResultOrParseError<EnumType> Enum(List<Token> tokens)
        {
            if (!MatchAndDiscard(TokenIdentifier.Identifier, "enum", tokens))
            {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("enum", tokens.FirstOrDefault()));
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens))
            {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            List<Identifier> result = new List<Identifier>();

            if (tokens.First().Identifier == TokenIdentifier.Identifier)
            {
                result.Add(tokens.First().Value);
                tokens.RemoveAt(0);
                while (MatchAndDiscard(TokenIdentifier.Symbol, ",", tokens))
                {
                    var id = tokens.FirstOrDefault();
                    if (id == null || id.Identifier != TokenIdentifier.Identifier)
                    {
                        return new ResultOrParseError<EnumType>(new ExpectedTokenParseError(TokenIdentifier.Identifier, id));
                    }

                    result.Add(id.Value);
                    tokens.RemoveAt(0);
                }
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens))
            {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return new EnumType(result);
        }

        internal static ResultOrParseError<PartialFunction> PartialParseFunctionBits(List<Token> tokens)
        {
            if (!tokens.Any())
            {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, null));
            }

            var identifiers = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new Identifier(t.Value)).ToList();
            if (!identifiers.Any())
            {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.First()));
            }

            tokens.RemoveRange(0, identifiers.Count());

            var partialBlock = PartialBlock(tokens);
            if (!partialBlock.Success)
            {
                return new ResultOrParseError<PartialFunction>(partialBlock.Error);
            }

            return new PartialFunction(identifiers, partialBlock.Result);
        }

        internal static ResultOrParseError<PartialBlock> PartialBlock(List<Token> tokens)
        {
            List<IEnumerable<PartialElement>> result = new List<IEnumerable<PartialElement>>();
            string closeTarget;

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens))
            {
                if (!MatchAndDiscard(TokenIdentifier.Symbol, "(", tokens))
                {
                    return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
                }
                else
                {
                    closeTarget = ")";
                }
            }
            else
            {
                closeTarget = "}";
            }

            while (tokens.Any() && tokens.First().Value != closeTarget)
            {
                var statement = PartialStatement(tokens);
                if (!statement.Success)
                {
                    return new ResultOrParseError<PartialBlock>(statement.Error);
                }

                result.Add(statement.Result);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, closeTarget, tokens))
            {
                return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError(closeTarget, tokens.FirstOrDefault()));
            }

            return new ResultOrParseError<PartialBlock>(new PartialBlock(result.Select(stmt => new PartialStatement(stmt))));
        }

        internal static ResultOrParseError<IEnumerable<PartialElement>> PartialStatement(List<Token> tokens)
        {
            List<PartialElement> result = new List<PartialElement>();
            while (true)
            {
                var first = tokens.FirstOrDefault();
                if (first == null)
                {
                    return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                }
                else if (first.Identifier == TokenIdentifier.Identifier)
                {
                    tokens.RemoveAt(0);
                    result.Add(new IdentifierElement(first.Value));
                }
                else if (first.Identifier == TokenIdentifier.StringConstant)
                {
                    tokens.RemoveAt(0);
                    result.Add(new ConstantElement<string>(new ConstantExpression<string>(TangentType.String, first.Value.Substring(1, first.Value.Length - 2))));
                }
                else if (first.Value == ";")
                {
                    if (result.Any())
                    {
                        tokens.RemoveAt(0);
                        return result;
                    }
                    else
                    {
                        return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                    }
                }
                else if (first.Value == "{" || first.Value == "(")
                {
                    var block = PartialBlock(tokens);
                    if (block.Success)
                    {
                        result.Add(new BlockElement(block.Result));
                    }
                    else
                    {
                        return new ResultOrParseError<IEnumerable<PartialElement>>(block.Error);
                    }
                }
                else if (first.Value == "}" || first.Value == ")")
                {
                    // we're at end of block. Return statement for optional semi-colon.
                    return result;
                }
                else
                {
                    return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                }
            }
        }

        private static bool MatchAndDiscard(TokenIdentifier id, string value, List<Token> tokens)
        {
            if (!tokens.Any() || tokens.First().Identifier != id || tokens.First().Value != value)
            {
                return false;
            }

            tokens.RemoveAt(0);
            return true;
        }
    }
}
