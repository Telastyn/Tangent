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
            if (!tokens.Any()) {
                return new TangentProgram(Enumerable.Empty<TypeDeclaration>(), Enumerable.Empty<ReductionDeclaration>(), Enumerable.Empty<string>());
            }

            List<string> inputSources = tokens.Select(t => t.SourceInfo.Label).Distinct().ToList();
            List<PartialTypeDeclaration> partialTypes = new List<PartialTypeDeclaration>();
            List<PartialReductionDeclaration> partialFunctions = new List<PartialReductionDeclaration>();

            while (tokens.Any()) {
                var error = ParseDeclaration(tokens, partialTypes, partialFunctions, null);
                if (error != null) {
                    return new ResultOrParseError<TangentProgram>(error);
                }
            }

            List<TypeDeclaration> builtInTypes = new List<TypeDeclaration>(){
                new TypeDeclaration("void", TangentType.Void),
                new TypeDeclaration("int", TangentType.Int),
                new TypeDeclaration("double", TangentType.Double),
                new TypeDeclaration("bool", TangentType.Bool),
                new TypeDeclaration("any", TangentType.Any)
            };

            Dictionary<PartialParameterDeclaration, ParameterDeclaration> genericArgumentMapping;
            var typeResult = TypeResolve.AllPartialTypeDeclarations(partialTypes, builtInTypes, out genericArgumentMapping);
            if (!typeResult.Success) {
                return new ResultOrParseError<Intermediate.TangentProgram>(typeResult.Error);
            }

            var types = typeResult.Result;

            // Move to Phase 2 - Resolve types in parameters and function return types.
            Dictionary<TangentType, TangentType> conversions;
            var resolvedTypes = TypeResolve.AllTypePlaceholders(types, genericArgumentMapping, out conversions);
            if (!resolvedTypes.Success) {
                return new ResultOrParseError<Intermediate.TangentProgram>(resolvedTypes.Error);
            }

            var resolvedFunctions = TypeResolve.AllPartialFunctionDeclarations(partialFunctions, resolvedTypes.Result, conversions);
            if (!resolvedFunctions.Success) {
                return new ResultOrParseError<TangentProgram>(resolvedFunctions.Error);
            }

            HashSet<ProductType> allProductTypes = new HashSet<ProductType>();
            foreach (var t in resolvedTypes.Result) {
                if (t.Returns.ImplementationType == KindOfType.Product) {
                    allProductTypes.Add((ProductType)t.Returns);
                } else if (t.Returns.ImplementationType == KindOfType.Sum) {
                    allProductTypes.UnionWith(((SumType)t.Returns).Types.Where(tt => tt.ImplementationType == KindOfType.Product).Cast<ProductType>());
                }
            }

            var ctorCalls = allProductTypes.Select(pt => new ReductionDeclaration(pt.DataConstructorParts, new CtorCall(pt))).ToList();
            //// RMS: leaving this here for now, as I'll probably need it again.
            //foreach (var sumTypeDeclaration in resolvedTypes.Result.Where(t => t.Returns.ImplementationType == KindOfType.Sum)) {
            //    var sumType = (sumTypeDeclaration.Returns as SumType);
            //    if (sumTypeDeclaration.IsGeneric) {
            //        var genericParams = sumTypeDeclaration.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
            //        var fnGenerics = genericParams.Select(pd => new ParameterDeclaration(pd.Takes, pd.Returns)).ToList();
            //        var inferences = fnGenerics.ToDictionary(p => p, p => GenericInferencePlaceholder.For(p));
            //        var genericReferences = fnGenerics.Select(pd => GenericArgumentReferenceType.For(pd)).ToList();
            //        var ctorReturnType = sumTypeDeclaration.Returns.ResolveGenericReferences(pd => genericReferences[genericParams.IndexOf(pd)]);

            //        foreach (var type in sumType.Types) {
            //            List<ParameterDeclaration> inferredTypes = new List<ParameterDeclaration>();
            //            var paramType = type.ResolveGenericReferences(pd => { inferredTypes.Add(pd); return inferences[fnGenerics[genericParams.IndexOf(pd)]]; });
            //            var specificCtorType = sumTypeDeclaration.Returns.ResolveGenericReferences(pd => inferredTypes.Contains(pd) ? genericReferences[genericParams.IndexOf(pd)] : TangentType.DontCare);

            //            ctorCalls.Add(new ReductionDeclaration(new[] { new PhrasePart(new ParameterDeclaration("_", paramType)) }, new CtorCall(specificCtorType as SumType), inferredTypes.Select(it => fnGenerics[genericParams.IndexOf(it)])));
            //        }
            //    } else {
            //        foreach (var type in sumType.Types) {
            //            ctorCalls.Add(new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", type)), new CtorCall(sumType)));
            //        }
            //    }
            //}

            // And now Phase 3 - Statement parsing based on syntax.
            var lookup = new Dictionary<Function, Function>();
            var bad = new List<IncomprehensibleStatementError>();
            var ambiguous = new List<AmbiguousStatementError>();
            resolvedFunctions = FanOutFunctionsWithSumTypes(resolvedFunctions.Result);
            if (!resolvedFunctions.Success) { return new ResultOrParseError<TangentProgram>(resolvedFunctions.Error); }

            foreach (var fn in resolvedFunctions.Result) {
                TypeResolvedFunction partialFunction = fn.Returns as TypeResolvedFunction;
                if (partialFunction != null) {
                    var scope = new Scope(partialFunction.EffectiveType, resolvedTypes.Result, fn.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), partialFunction.Scope != null ? partialFunction.Scope.DataConstructorParts.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter) : Enumerable.Empty<ParameterDeclaration>(), resolvedFunctions.Result.Concat(BuiltinFunctions.All).Concat(ctorCalls), Enumerable.Empty<ParameterDeclaration>());
                    Function newb = BuildBlock(scope, partialFunction.EffectiveType, partialFunction.Implementation, bad, ambiguous);

                    lookup.Add(partialFunction, newb);
                } else {
                    throw new NotImplementedException("We shouldn't get here... No optimizations exist to fully resolve functions in one step.");
                }
            }

            if (bad.Any() || ambiguous.Any()) {
                return new ResultOrParseError<TangentProgram>(new StatementGrokErrors(bad, ambiguous));
            }

            // 3a - Replace TypeResolvedFunctions with fully resolved ones.
            var workset = new HashSet<Expression>();
            foreach (var fn in lookup.Values) {
                fn.ReplaceTypeResolvedFunctions(lookup, workset);
            }

            return new TangentProgram(resolvedTypes.Result, resolvedFunctions.Result.Select(fn =>
            {
                if (fn.Returns is TypeResolvedFunction) {
                    return new ReductionDeclaration(fn.Takes, lookup[fn.Returns]);
                } else {
                    return fn;
                }
            }).ToList(), inputSources);
        }

        private static ParseError ParseDeclaration(List<Token> tokens, List<PartialTypeDeclaration> partialTypes, List<PartialReductionDeclaration> partialFunctions, PartialProductType scope)
        {
            var shouldBePhrase = PartialPhrase(tokens, scope != null);
            if (!shouldBePhrase.Success) {
                return shouldBePhrase.Error;
            }

            var phrase = shouldBePhrase.Result;

            var separator = tokens.First();
            tokens.RemoveAt(0);
            switch (separator.Identifier) {
                case TokenIdentifier.TypeDeclSeparator:
                    if (partialTypes == null) { return new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator); }

                    // Normalize generics
                    phrase = phrase.Select(pp => pp.IsIdentifier ? pp : (pp.Parameter.Returns != null ? pp : new PartialPhrasePart(new PartialParameterDeclaration(pp.Parameter.Takes, new List<IdentifierExpression>() { new IdentifierExpression("any", null) })))).ToList();
                    if (phrase.All(pp => !pp.IsIdentifier)) { return new ExpectedTokenParseError(TokenIdentifier.Identifier, separator); }
                    if (phrase.Any(pp => !pp.IsIdentifier && pp.Parameter.Takes.Count == 1 && pp.Parameter.Takes.First() == "this")) { return new ThisAsGeneric(); }

                    var typeDecl = Type(tokens, phrase.Where(ppp => !ppp.IsIdentifier).Select(ppp => ppp.Parameter));
                    if (!typeDecl.Success) {
                        return typeDecl.Error;
                    }

                    partialFunctions.AddRange(ExtractPartialFunctions(typeDecl.Result));
                    partialTypes.Add(new PartialTypeDeclaration(phrase, typeDecl.Result));
                    break;
                case TokenIdentifier.ReductionDeclSeparator:
                    if (shouldBePhrase.Result.Any(pp => !pp.IsIdentifier && pp.Parameter.Returns == null)) {
                        throw new NotImplementedException("Sorry, generic functions are not currently supported.");
                    }

                    var functionParts = PartialParseFunctionBits(tokens, scope);
                    if (!functionParts.Success) {
                        return functionParts.Error;
                    }

                    partialFunctions.Add(new PartialReductionDeclaration(phrase, functionParts.Result));
                    break;
                default:
                    return new ExpectedTokenParseError(TokenIdentifier.ReductionDeclSeparator, separator);
            }

            return null;

        }

        private static Function BuildBlock(Scope scope, TangentType effectiveType, PartialBlock partialBlock, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            var block = BuildBlock(scope, effectiveType, partialBlock.Statements, bad, ambiguous);

            return new Function(effectiveType, block);
        }

        private static Block BuildBlock(Scope scope, TangentType effectiveType, IEnumerable<PartialStatement> elements, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            List<Expression> statements = new List<Expression>();
            if (!elements.Any()) {
                if (effectiveType != TangentType.Void) { bad.Add(new IncomprehensibleStatementError(Enumerable.Empty<Expression>())); }
                return new Block(Enumerable.Empty<Expression>());
            }

            var allElements = elements.ToList();
            for (int ix = 0; ix < allElements.Count; ++ix) {
                var line = allElements[ix];
                var statementBits = line.FlatTokens.Select(t => ElementToExpression(scope, t, bad, ambiguous));
                var statement = new Input(statementBits, scope).InterpretTowards((effectiveType != null && ix == allElements.Count - 1) ? effectiveType : TangentType.Void);
                if (statement.Count == 0) {
                    bad.Add(new IncomprehensibleStatementError(statementBits));
                } else if (statement.Count > 1) {
                    ambiguous.Add(new AmbiguousStatementError(statementBits, statement));
                } else {
                    statements.Add(statement.First());
                }
            }

            return new Block(statements);
        }

        private static Expression ElementToExpression(Scope scope, PartialElement element, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            switch (element.Type) {
                case ElementType.Identifier:
                    return new IdentifierExpression(((IdentifierElement)element).Identifier, element.SourceInfo);
                case ElementType.Parens:
                    throw new NotImplementedException("Parens expressions not yet supported.");
                case ElementType.Block:
                    var stmts = ((BlockElement)element).Block.Statements.ToList();
                    var last = stmts.Last();
                    stmts.RemoveAt(stmts.Count - 1);
                    var notLast = BuildBlock(scope, null, stmts, bad, ambiguous);
                    var lastExpr = last.FlatTokens.Select(e => ElementToExpression(scope, e, bad, ambiguous)).ToList();
                    var info = lastExpr.Aggregate((LineColumnRange)null, (a, expr) => expr.SourceInfo.Combine(a));
                    info = notLast.Statements.Any() ? notLast.Statements.Aggregate(info, (a, stmt) => a.Combine(stmt.SourceInfo)) : info;
                    return new ParenExpression(notLast, lastExpr, info);
                case ElementType.Constant:
                    return ((ConstantElement)element).TypelessExpression;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static ResultOrParseError<List<PartialPhrasePart>> PartialPhrase(List<Token> tokens, bool classDecl)
        {
            var phrase = new List<PartialPhrasePart>();

            ResultOrParseError<PartialPhrasePart> part = null;
            while (true) {
                part = TryPartialPhrasePart(tokens, classDecl);
                if (part == null) {
                    if (phrase.Any()) {
                        if (classDecl && !phrase.Any(pp => !pp.IsIdentifier && pp.Parameter.IsThisParam)) {
                            return new ResultOrParseError<List<PartialPhrasePart>>(new ExpectedLiteralParseError("this", tokens.FirstOrDefault()));
                        }

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

        internal static ResultOrParseError<PartialPhrasePart> TryPartialPhrasePart(List<Token> tokens, bool classDecl)
        {
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
                var paramName = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new IdentifierExpression(t.Value, t.SourceInfo)).ToList();
                tokens.RemoveRange(0, paramName.Count);
                if (paramName.Count == 0) {
                    return new ResultOrParseError<PartialPhrasePart>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.FirstOrDefault()));
                }

                if (classDecl && paramName.Count == 1 && paramName.First().Identifier.Value == "this" && tokens.Any() && tokens.First().Value == ")") {
                    tokens.RemoveAt(0);
                    return new PartialPhrasePart(new PartialParameterDeclaration("this", paramName));
                } else {

                    var possibleColon = tokens.FirstOrDefault();
                    if (possibleColon != null && possibleColon.Value == ")") {
                        // Something like List(T) - return null as the type info.
                        tokens.RemoveAt(0);
                        return new ResultOrParseError<PartialPhrasePart>(new PartialPhrasePart(new PartialParameterDeclaration(paramName.Select(p => p.Identifier), null)));
                    }

                    if (possibleColon == null || possibleColon.Value != ":") {
                        return new ResultOrParseError<PartialPhrasePart>(new ExpectedLiteralParseError(":", possibleColon));
                    }

                    tokens.RemoveAt(0);

                    var typeName = PartialStatement(tokens, ")");
                    if (!typeName.Success) {
                        return new ResultOrParseError<PartialPhrasePart>(typeName.Error);
                    }

                    if (!typeName.Result.All(pe => pe.Type == ElementType.Identifier)) {
                        throw new NotImplementedException("sorry, non-identifier/symbols in type expressions is not currently supported.");
                    }

                    return new PartialPhrasePart(new PartialParameterDeclaration(paramName.Select(p => p.Identifier), typeName.Result.Select(pe => new IdentifierExpression(((IdentifierElement)pe).Identifier, pe.SourceInfo)).ToList()));
                }
            }

            if (first.Identifier == TokenIdentifier.Symbol && first.Value != "{" && first.Value != "|") {
                if (first.Value == ";") {
                    tokens.RemoveAt(0);
                    return null;
                }

                tokens.RemoveAt(0);
                return new PartialPhrasePart(first.Value);
            }

            return null;
        }

        public static ResultOrParseError<TangentType> Type(List<Token> tokens, IEnumerable<PartialParameterDeclaration> genericArgs)
        {
            var enumResult = Enum(tokens);
            if (enumResult.Success) {
                return TryExtendSumType(tokens, genericArgs, enumResult.Result);
            } else {
                var classResult = Class(tokens, genericArgs);
                if (classResult.Success) {
                    return TryExtendSumType(tokens, genericArgs, classResult.Result);
                }

                return new ResultOrParseError<TangentType>(classResult.Error);
            }
        }

        private static ResultOrParseError<TangentType> TryExtendSumType(List<Token> tokens, IEnumerable<PartialParameterDeclaration> genericArgs, TangentType firstPart)
        {
            if (!tokens.Any()) { return firstPart; }
            if (tokens[0].Identifier == TokenIdentifier.Symbol && tokens[0].Value == "|") {
                tokens.RemoveAt(0);
                ResultOrParseError<TangentType> result = Type(tokens, genericArgs);
                if (result.Success) {
                    if (result.Result.ImplementationType == KindOfType.Sum) {
                        return SumType.For(((SumType)result.Result).Types.Concat(new List<TangentType>() { firstPart }));
                    } else {
                        return SumType.For(new List<TangentType>() { firstPart, result.Result });
                    }
                } else {
                    return result;
                }
            }

            return firstPart;
        }

        public static ResultOrParseError<EnumType> Enum(List<Token> tokens)
        {
            if (!MatchAndDiscard(TokenIdentifier.Identifier, "enum", tokens)) {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("enum", tokens.FirstOrDefault()));
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            List<Identifier> result = new List<Identifier>();

            if (tokens.First().Identifier == TokenIdentifier.Identifier) {
                result.Add(tokens.First().Value);
                tokens.RemoveAt(0);
                while (MatchAndDiscard(TokenIdentifier.Symbol, ",", tokens)) {
                    var id = tokens.FirstOrDefault();
                    if (id == null || id.Identifier != TokenIdentifier.Identifier) {
                        return new ResultOrParseError<EnumType>(new ExpectedTokenParseError(TokenIdentifier.Identifier, id));
                    }

                    result.Add(id.Value);
                    tokens.RemoveAt(0);
                }
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens)) {
                return new ResultOrParseError<EnumType>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return new EnumType(result);
        }

        internal static ResultOrParseError<TangentType> Class(List<Token> tokens, IEnumerable<PartialParameterDeclaration> genericArgs)
        {
            var phrasePart = PartialPhrase(tokens, false);
            if (!phrasePart.Success) {
                return new ResultOrParseError<TangentType>(phrasePart.Error);
            }

            if (phrasePart.Result.All(pp => pp.IsIdentifier) && (!tokens.Any() || tokens[0].Value != "{")) {
                return new PartialTypeReference(phrasePart.Result.Select(pp => new IdentifierExpression(pp.Identifier, null)), genericArgs);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                return new ResultOrParseError<TangentType>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
            }

            var result = new PartialProductType(phrasePart.Result, Enumerable.Empty<PartialReductionDeclaration>(), genericArgs);

            while (tokens.Any() && tokens.First().Value != "}") {
                var error = ParseDeclaration(tokens, null, result.Functions, result);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "}", tokens)) {
                return new ResultOrParseError<TangentType>(new ExpectedLiteralParseError("}", tokens.FirstOrDefault()));
            }

            return result;
        }

        internal static ResultOrParseError<PartialFunction> PartialParseFunctionBits(List<Token> tokens, PartialProductType scope)
        {
            if (!tokens.Any()) {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, null));
            }

            var identifiers = tokens.TakeWhile(t => t.Identifier == TokenIdentifier.Identifier).Select(t => new IdentifierExpression(t.Value, t.SourceInfo)).ToList();
            if (!identifiers.Any()) {
                return new ResultOrParseError<PartialFunction>(new ExpectedTokenParseError(TokenIdentifier.Identifier, tokens.First()));
            }

            tokens.RemoveRange(0, identifiers.Count());

            var partialBlock = PartialBlock(tokens);
            if (!partialBlock.Success) {
                return new ResultOrParseError<PartialFunction>(partialBlock.Error);
            }

            return new PartialFunction(identifiers, partialBlock.Result, scope);
        }

        internal static ResultOrParseError<PartialBlock> PartialBlock(List<Token> tokens)
        {
            List<IEnumerable<PartialElement>> result = new List<IEnumerable<PartialElement>>();
            string closeTarget;

            if (!MatchAndDiscard(TokenIdentifier.Symbol, "{", tokens)) {
                if (!MatchAndDiscard(TokenIdentifier.Symbol, "(", tokens)) {
                    return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError("{", tokens.FirstOrDefault()));
                } else {
                    closeTarget = ")";
                }
            } else {
                closeTarget = "}";
            }

            while (tokens.Any() && tokens.First().Value != closeTarget) {
                var statement = PartialStatement(tokens);
                if (!statement.Success) {
                    return new ResultOrParseError<PartialBlock>(statement.Error);
                }

                result.Add(statement.Result);
            }

            if (!MatchAndDiscard(TokenIdentifier.Symbol, closeTarget, tokens)) {
                return new ResultOrParseError<PartialBlock>(new ExpectedLiteralParseError(closeTarget, tokens.FirstOrDefault()));
            }

            return new ResultOrParseError<PartialBlock>(new PartialBlock(result.Select(stmt => new PartialStatement(stmt))));
        }

        internal static ResultOrParseError<IEnumerable<PartialElement>> PartialStatement(List<Token> tokens, string statementTerminator = ";")
        {
            List<PartialElement> result = new List<PartialElement>();
            while (true) {
                var first = tokens.FirstOrDefault();

                if (first == null) {
                    return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                } else if (first.Identifier == TokenIdentifier.Identifier) {
                    tokens.RemoveAt(0);
                    result.Add(new IdentifierElement(first.Value, first.SourceInfo));
                } else if (first.Identifier == TokenIdentifier.StringConstant) {
                    tokens.RemoveAt(0);
                    result.Add(new ConstantElement<string>(new ConstantExpression<string>(TangentType.String, first.Value.Substring(1, first.Value.Length - 2), first.SourceInfo)));
                } else if (first.Identifier == TokenIdentifier.IntegerConstant) {
                    tokens.RemoveAt(0);
                    result.Add(new ConstantElement<int>(new ConstantExpression<int>(TangentType.Int, int.Parse(first.Value), first.SourceInfo)));
                } else if (first.Value == statementTerminator) {
                    if (result.Any()) {
                        tokens.RemoveAt(0);
                        return result;
                    } else {
                        return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                    }
                } else if (first.Value == "{" || first.Value == "(") {
                    var block = PartialBlock(tokens);
                    if (block.Success) {
                        result.Add(new BlockElement(block.Result));
                    } else {
                        return new ResultOrParseError<IEnumerable<PartialElement>>(block.Error);
                    }
                } else if (first.Value == "}" || first.Value == ")") {
                    // we're at end of block. Return statement for optional semi-colon.
                    return result;
                } else if (first.Identifier == TokenIdentifier.Symbol) {
                    tokens.RemoveAt(0);
                    result.Add(new IdentifierElement(first.Value, first.SourceInfo));
                } else {
                    return new ResultOrParseError<IEnumerable<PartialElement>>(new ExpectedTokenParseError(TokenIdentifier.Identifier, first));
                }
            }
        }

        private static bool MatchAndDiscard(TokenIdentifier id, string value, List<Token> tokens)
        {
            if (!tokens.Any() || tokens.First().Identifier != id || tokens.First().Value != value) {
                return false;
            }

            tokens.RemoveAt(0);
            return true;
        }

        private static IEnumerable<PartialReductionDeclaration> ExtractPartialFunctions(TangentType tt, HashSet<TangentType> searched = null)
        {
            searched = searched ?? new HashSet<TangentType>();
            switch (tt.ImplementationType) {
                case KindOfType.Sum:
                    if (searched.Contains(tt)) {
                        return Enumerable.Empty<PartialReductionDeclaration>();
                    }

                    searched.Add(tt);
                    List<PartialReductionDeclaration> result = new List<PartialReductionDeclaration>();
                    foreach (var part in ((SumType)tt).Types) {
                        result.AddRange(ExtractPartialFunctions(part, searched));
                    }

                    return result;
                case KindOfType.Placeholder:
                    if (tt is PartialProductType) {
                        return ((PartialProductType)tt).Functions;
                    }

                    return Enumerable.Empty<PartialReductionDeclaration>();
                default:
                    return Enumerable.Empty<PartialReductionDeclaration>();
            }
        }


        private static ResultOrParseError<IEnumerable<ReductionDeclaration>> FanOutFunctionsWithSumTypes(IEnumerable<ReductionDeclaration> resolvedFunctions)
        {
            List<ReductionDeclaration> result = new List<ReductionDeclaration>(resolvedFunctions);
            for (int ix = 0; ix < result.Count; ++ix) {
                var entry = result[ix];

                List<List<PhrasePart>> parts = entry.Takes.Select(pp =>
                {
                    if (!pp.IsIdentifier && pp.Parameter.Returns.ImplementationType == KindOfType.Sum) {
                        return ((SumType)pp.Parameter.Returns).Types.Select(tt => new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, tt))).ToList();
                    } else if (!pp.IsIdentifier && pp.Parameter.Returns.ImplementationType == KindOfType.BoundGeneric) {
                        var conc = pp.Parameter.Returns;
                        while (conc.ImplementationType == KindOfType.BoundGeneric) {
                            conc = ((BoundGenericType)conc).ConcreteType;
                        }

                        if (conc.ImplementationType == KindOfType.Sum) {
                            return ((SumType)conc).Types.Select(tt => new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, tt))).ToList();
                        }

                        return new List<PhrasePart>() { pp };

                    } else {
                        return new List<PhrasePart>() { pp };
                    }
                }).ToList();

                if (!parts.All(p => p.Count == 1)) {
                    foreach (var variant in parts.GetCombos()) {
                        var trf = entry.Returns as TypeResolvedFunction;
                        if (trf == null) { throw new NotImplementedException(); }
                        var parameterGenerics = variant.SelectMany(pp => pp.IsIdentifier ? Enumerable.Empty<ParameterDeclaration>() : pp.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference)).ToList();
                        var returnGenericsTiedToInference = trf.EffectiveType.ContainedGenericReferences(GenericTie.Reference).Where(pd => entry.GenericParameters.Contains(pd));
                        var badGenerics = returnGenericsTiedToInference.Where(pd => !parameterGenerics.Contains(pd)).ToList();
                        if (badGenerics.Count == 1) {
                            return new ResultOrParseError<IEnumerable<ReductionDeclaration>>(new GenericSumTypeFunctionWithReturnTypeRelyingOnInference(variant, badGenerics[0]));
                        }

                        if (badGenerics.Count > 1) {
                            return new ResultOrParseError<IEnumerable<ReductionDeclaration>>(new AggregateParseError(badGenerics.Select(bg => new GenericSumTypeFunctionWithReturnTypeRelyingOnInference(variant, bg))));
                        }

                        result.Add(new ReductionDeclaration(variant, new TypeResolvedFunction(trf.EffectiveType, trf.Implementation, trf.Scope), parameterGenerics));
                    }
                }
            }
            

            return result;
        }
    }
}
