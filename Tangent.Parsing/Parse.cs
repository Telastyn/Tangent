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
                int typeTake;
                var type = Grammar.TypeDecl.Parse(tokens, out typeTake);
                int fnTake;
                if (type.Success) {
                    partialTypes.Add(type.Result);
                    partialFunctions.AddRange(ExtractPartialFunctions(type.Result.Returns));
                    tokens.RemoveRange(0, typeTake);
                } else {
                    var fn = Grammar.FunctionDeclaration.Parse(tokens, out fnTake);
                    if (fn.Success) {
                        partialFunctions.Add(fn.Result);
                        tokens.RemoveRange(0, fnTake);
                    } else {
                        return new ResultOrParseError<TangentProgram>(typeTake >= fnTake ? type.Error : fn.Error);
                    }
                }
            }

            List<TypeDeclaration> builtInTypes = new List<TypeDeclaration>(){
                new TypeDeclaration("void", TangentType.Void),
                new TypeDeclaration("int", TangentType.Int),
                new TypeDeclaration("double", TangentType.Double),
                new TypeDeclaration("bool", TangentType.Bool),
                new TypeDeclaration("string", TangentType.String),
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

            HashSet<SumType> allSumTypes = new HashSet<SumType>(resolvedTypes.Result.Where(t => t.Returns.ImplementationType == KindOfType.Sum).Select(t => t.Returns).Cast<SumType>());

            var ctorCalls = allProductTypes.Select(pt => new ReductionDeclaration(pt.DataConstructorParts, new CtorCall(pt), pt.DataConstructorParts.SelectMany(pp => pp.IsIdentifier ? Enumerable.Empty<ParameterDeclaration>() : pp.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference)))).ToList();
            foreach (var sum in allSumTypes) {
                foreach (var entry in sum.Types) {
                    ctorCalls.Add(new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", entry)), new CtorCall(sum)));
                }
            }

            var enumAccesses = resolvedTypes.Result.Where(tt => tt.Returns.ImplementationType == KindOfType.Enum).Select(tt => tt.Returns).Cast<EnumType>().SelectMany(tt => tt.Values.Select(v => new ReductionDeclaration(v, new Function(tt, new Block(new[] { new EnumValueAccessExpression(tt.SingleValueTypeFor(v), null) }))))).ToList();


            // And now Phase 3 - Statement parsing based on syntax.
            var lookup = new Dictionary<Function, Function>();
            var bad = new List<IncomprehensibleStatementError>();
            var ambiguous = new List<AmbiguousStatementError>();
            resolvedFunctions = new ResultOrParseError<IEnumerable<ReductionDeclaration>>(resolvedFunctions.Result.Concat(BuiltinFunctions.All).Concat(enumAccesses));
            resolvedFunctions = FanOutFunctionsWithSumTypes(resolvedFunctions.Result);
            if (!resolvedFunctions.Success) { return new ResultOrParseError<TangentProgram>(resolvedFunctions.Error); }

            foreach (var fn in resolvedFunctions.Result) {
                TypeResolvedFunction partialFunction = fn.Returns as TypeResolvedFunction;
                if (partialFunction != null) {
                    var scope = new TransformationScope(((IEnumerable<TransformationRule>)resolvedTypes.Result.Select(td => new TypeAccess(td)))
                        .Concat(fn.Takes.Where(pp => !pp.IsIdentifier).Select(pp => new ParameterAccess(pp.Parameter)))
                        .Concat((partialFunction.Scope as ProductType) != null ? ConstructorParameterAccess.For(fn.Takes.First(pp => !pp.IsIdentifier && pp.Parameter.Takes.Count == 1 && pp.Parameter.IsThisParam).Parameter, (partialFunction.Scope as ProductType).DataConstructorParts.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter)) : Enumerable.Empty<TransformationRule>())
                        .Concat(resolvedFunctions.Result.Concat(ctorCalls).Select(f => new FunctionInvocation(f)))
                        .Concat(new TransformationRule[] { LazyOperator.Common, SingleValueAccessor.Common, Delazy.Common }));

                    Function newb = BuildBlock(scope, partialFunction.EffectiveType, partialFunction.Implementation, bad, ambiguous);

                    lookup.Add(partialFunction, newb);
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
                    return new ReductionDeclaration(fn.Takes, lookup[fn.Returns], fn.GenericParameters);
                } else {
                    return fn;
                }
            }).ToList(), inputSources);
        }

        private static Function BuildBlock(TransformationScope scope, TangentType effectiveType, PartialBlock partialBlock, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            var block = BuildBlock(scope, effectiveType, partialBlock.Statements, bad, ambiguous);

            return new Function(effectiveType, block);
        }

        private static Block BuildBlock(TransformationScope scope, TangentType effectiveType, IEnumerable<PartialStatement> elements, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            List<Expression> statements = new List<Expression>();
            if (!elements.Any()) {
                if (effectiveType != TangentType.Void) { bad.Add(new IncomprehensibleStatementError(Enumerable.Empty<Expression>())); }
                return new Block(Enumerable.Empty<Expression>());
            }

            var allElements = elements.ToList();
            for (int ix = 0; ix < allElements.Count; ++ix) {
                var line = allElements[ix];
                var statementBits = line.FlatTokens.Select(t => ElementToExpression(scope, t, bad, ambiguous)).ToList();
                var statement = scope.InterpretTowards((effectiveType != null && ix == allElements.Count - 1) ? effectiveType : TangentType.Void, statementBits);
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

        private static Expression ElementToExpression(TransformationScope scope, PartialElement element, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            switch (element.Type) {
                case ElementType.Identifier:
                    return new IdentifierExpression(((IdentifierElement)element).Identifier, element.SourceInfo);
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
                case ElementType.Lambda:
                    var concrete = (LambdaElement)element;
                    return new PartialLambdaExpression(concrete.Takes.Select(vde => VarDeclToParameterDeclaration(scope, vde, bad, ambiguous)).ToList(), scope, (newScope, returnType) =>
                    {
                        var errors = new List<IncomprehensibleStatementError>();
                        var ambiguities = new List<AmbiguousStatementError>();
                        var implementation = BuildBlock(newScope, returnType, concrete.Body.Block, errors, ambiguities);
                        if (errors.Any()) {
                            return null;
                        }

                        if (ambiguities.Any()) {
                            return new AmbiguousExpression(ambiguities.SelectMany(a => a.PossibleInterpretations));
                        }

                        // RMS: being lazy. Should probably have an Either or a BlockExpr.
                        return new ParenExpression(implementation.Implementation, null, concrete.Body.SourceInfo);
                    }, element.SourceInfo);
                default:
                    throw new NotImplementedException();
            }
        }

        private static ParameterDeclaration VarDeclToParameterDeclaration(TransformationScope scope, VarDeclElement vde, List<IncomprehensibleStatementError> bad, List<AmbiguousStatementError> ambiguous)
        {
            if (!vde.ParameterDeclaration.Takes.All(ppp => ppp.IsIdentifier)) {
                throw new NotImplementedException("Parameterized variable declarations not currently supported.");
            }

            var result = vde.ParameterDeclaration.Returns == null ? new ParameterDeclaration(vde.ParameterDeclaration.Takes.Select(ppp => new PhrasePart(ppp.Identifier.Identifier)), null) :
                TypeResolve.Resolve(vde.ParameterDeclaration, scope.Rules.SelectMany(x => x).Where(r => r.Type == TransformationType.Type).Cast<TypeAccess>().Select(t => t.Declaration));
            if (result.Success) {
                return result.Result;
            } else {
                bad.Add(new IncomprehensibleStatementError(vde.ParameterDeclaration.Returns));
                return null;
            }
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

                    if(tt is PartialInterface) {
                        return ((PartialInterface)tt).Functions;
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

                        var newb = new ReductionDeclaration(variant, new TypeResolvedFunction(trf.EffectiveType, trf.Implementation, trf.Scope), parameterGenerics);
                        // Check if some specialization already exists for this variant.
                        if (!result.Any(fn => fn.MatchesSignatureOf(newb))) {
                            result.Add(newb);
                        }
                    }
                }
            }

            return result;
        }
    }
}
