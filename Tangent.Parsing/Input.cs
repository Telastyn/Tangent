using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public class Input
    {
        private readonly List<Expression> buffer;
        public readonly Scope Scope;
        private readonly List<ConversionHistory> conversionsTaken;
        private readonly IEnumerable<TransformationRule> customTransformations;

        public Input(IEnumerable<Identifier> identifiers, Scope scope, IEnumerable<TransformationRule> customTransformations = null)
        {
            this.customTransformations = customTransformations ?? Enumerable.Empty<TransformationRule>();
            buffer = new List<Expression>(identifiers.Select(id => new IdentifierExpression(id, null)));
            Scope = scope;
            conversionsTaken = new List<ConversionHistory>();
        }

        public Input(IEnumerable<Expression> exprs, Scope scope, IEnumerable<TransformationRule> customTransformations = null) : this(exprs, scope, new List<ConversionHistory>(), customTransformations) { }

        private Input(IEnumerable<Expression> exprs, Scope scope, IEnumerable<ConversionHistory> conversionsTaken, IEnumerable<TransformationRule> customTransformations)
        {
            buffer = new List<Expression>(exprs);
            Scope = scope;
            this.conversionsTaken = new List<ConversionHistory>(conversionsTaken);
            this.customTransformations = customTransformations ?? Enumerable.Empty<TransformationRule>();
        }

        public List<Expression> InterpretAsStatement()
        {
            return InterpretTowards(TangentType.Void);
        }

        internal List<Expression> InterpretTowards(TangentType type)
        {
            if (buffer.Count == 1) {
                if (type == buffer[0].EffectiveType) {
                    return buffer;
                } else if (type == TangentType.Any.Kind && (buffer[0].EffectiveType is KindType || buffer[0].EffectiveType is TypeConstant || buffer[0].EffectiveType is GenericArgumentReferenceType || buffer[0].EffectiveType is GenericInferencePlaceholder)) {
                    // mild hack since there's no subtyping yet.
                    return buffer;
                }
            }

            for (int ix = 0; ix < buffer.Count; ++ix) {
                foreach (var reductionCandidatePool in TryReduce(ix)) {
                    List<Expression> successes = new List<Expression>();
                    foreach (var candidate in reductionCandidatePool) {
                        if (candidate[ix].NodeType == ExpressionNodeType.FunctionInvocation && ((FunctionInvocationExpression)candidate[ix]).FunctionDefinition.IsConversion) {
                            successes.AddRange(new Input(candidate, Scope, conversionsTaken.Concat(new[] { new ConversionHistory(((FunctionInvocationExpression)candidate[ix]).FunctionDefinition, buffer.Count, ix) }), customTransformations).InterpretTowards(type));
                        } else {
                            successes.AddRange(new Input(candidate, Scope, conversionsTaken, customTransformations).InterpretTowards(type));
                        }
                    }

                    if (successes.Any()) {
                        return successes;
                    }
                }
            }

            // And if we've gotten here, we have some bundle of tokens that cannot be reduced further but don't result in our target.
            return new List<Expression>();
        }

        /// <summary>
        /// Checks for a match at index ix, yielding tiers of flattened expressions with the match processed.
        /// </summary>
        private IEnumerable<List<List<Expression>>> TryReduce(int ix)
        {
            // custom transforms
            var subBuffer = buffer.Skip(ix).ToList();
            foreach (var transform in customTransformations) {
                var result = transform.TryReduce(subBuffer);
                if (result != null) {
                    yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { result.ReplacesWith }).Concat(subBuffer.Skip(result.Takes)).ToList() };
                }
            }

            // generic param
            foreach (IGrouping<int, ParameterDeclaration> genericTier in Scope.GenericArguments.Where(pd => IsMatch(subBuffer, pd.Takes.Select(id => new PhrasePart(id)).ToList())).GroupBy(pd => pd.Takes.Count).OrderByDescending(grp => grp.Key)) {
                yield return genericTier.Select(pd => buffer.Take(ix).Concat(new[] { new GenericParameterAccessExpression(pd, buffer[ix].SourceInfo) }).Concat(buffer.Skip(ix + pd.Takes.Count)).ToList()).ToList();
            }

            // param
            foreach (IGrouping<int, ParameterDeclaration> parameterTier in Scope.Parameters.Where(pd => IsMatch(subBuffer, pd.Takes.Select(id => new PhrasePart(id)).ToList())).GroupBy(pd => pd.Takes.Count).OrderByDescending(grp => grp.Key)) {
                yield return parameterTier.Select(pd => buffer.Take(ix).Concat(new[] { new ParameterAccessExpression(pd, buffer[ix].SourceInfo) }).Concat(buffer.Skip(ix + pd.Takes.Count)).ToList()).ToList();
            }

            // ctor param
            foreach (IGrouping<int, ParameterDeclaration> ctorParameterTier in Scope.CtorParameters.Where(pd => IsMatch(subBuffer, pd.Takes.Select(id => new PhrasePart(id)).ToList())).GroupBy(pd => pd.Takes.Count).OrderByDescending(grp => grp.Key)) {
                yield return ctorParameterTier.Select(pd => buffer.Take(ix).Concat(new[] { new CtorParameterAccessExpression(Scope.Parameters.First(ctorPd => ctorPd.Takes.Count == 1 && ctorPd.Takes.First().Value == "this"), pd, buffer[ix].SourceInfo) }).Concat(buffer.Skip(ix + pd.Takes.Count)).ToList()).ToList();
            }

            // type
            foreach (IGrouping<int, TypeDeclaration> typeTier in Scope.Types.Where(td => IsMatch(buffer.Skip(ix).ToList(), td.Takes)).GroupBy(td => td.Takes.Count).OrderByDescending(grp => grp.Key)) {
                yield return typeTier.Select(td => buffer.Take(ix).Concat(new[] { new TypeAccessExpression(
                    (
                        td.IsGeneric?
                        BoundGenericType.For(td, buffer.Skip(ix).Where(expr=>!(expr is IdentifierExpression)).Take(td.Takes.Where(pp=>!pp.IsIdentifier).Count()).Select(expr=>expr.EffectiveType.ImplementationType == KindOfType.TypeConstant? ((TypeConstant)expr.EffectiveType).Value: expr.EffectiveType)) :
                        td.Returns
                    ).TypeConstant, buffer[ix].SourceInfo) }).Concat(buffer.Skip(ix + td.Takes.Count)).ToList()).ToList();
            }

            // fn
            var legalFunctionTypeInferences = new Dictionary<ReductionDeclaration, Dictionary<ParameterDeclaration, TangentType>>();
            var legalFunctions = Scope.Functions.Where(fn => !(fn.IsConversion && conversionsTaken.Contains(new ConversionHistory(fn, buffer.Count, ix)))).ToList();
            foreach (var fn in legalFunctions) {
                legalFunctionTypeInferences.Add(fn, new Dictionary<ParameterDeclaration, TangentType>());
            }

            legalFunctions = legalFunctions.Where(fn => IsMatch(buffer.Skip(ix).ToList(), fn.Takes, legalFunctionTypeInferences[fn])).ToList();

            while (legalFunctions.Any()) {
                var candidates = PopBestCandidates(legalFunctions);
                var pool = new List<List<Expression>>();
                foreach (var candidate in candidates) {
                    var bindings = PermutateParenBindings(
                        candidate.Takes.Where(t => !t.IsIdentifier).ToList(),
                        buffer.Skip(ix).Take(candidate.Takes.Count).Where(p => p.NodeType != ExpressionNodeType.Identifier).ToList(),
                        legalFunctionTypeInferences[candidate]);

                    foreach (var paramSet in bindings) {
                        pool.Add(buffer.Take(ix).Concat(new[] { new FunctionInvocationExpression(candidate, paramSet, candidate.GenericParameters.Select(p => legalFunctionTypeInferences[candidate][p]), LineColumnRange.Combine(buffer[ix].SourceInfo, paramSet.Select(ps => ps.SourceInfo))) }).Concat(buffer.Skip(ix + candidate.Takes.Count)).ToList());
                    }
                }

                yield return pool;
            }

            // enums
            if (buffer[ix].NodeType == ExpressionNodeType.Identifier) {
                var id = (buffer[ix] as IdentifierExpression).Identifier;
                var valueCandidates = Scope.Types.Where(td => td.Returns.ImplementationType == KindOfType.Enum && ((EnumType)td.Returns).Values.Select(v => v.Value).Contains(id.Value)).Select(td => td.Returns).Cast<EnumType>().Select(enumtype => enumtype.SingleValueTypeFor(id)).ToList();
                if (valueCandidates.Any()) {
                    yield return valueCandidates.Select(svt => buffer.Take(ix).Concat(new[] { new EnumValueAccessExpression(svt, buffer[ix].SourceInfo) }.Concat(buffer.Skip(ix + 1))).ToList()).ToList();
                }
            }

            // **Coersions.**
            // Enum constant -> Enum type
            if (buffer[ix].NodeType == ExpressionNodeType.EnumValueAccess) {
                yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { new EnumWideningExpression(buffer[ix] as EnumValueAccessExpression) }).Concat(buffer.Skip(ix + 1)).ToList() };
            }

            // Param accessed delegate -> delegate invocation.
            if (buffer[ix].NodeType == ExpressionNodeType.ParameterAccess) {
                var paramAccess = (ParameterAccessExpression)buffer[ix];
                if (paramAccess.Parameter.Returns.ImplementationType == KindOfType.Lazy) {
                    yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { new DelegateInvocationExpression(paramAccess) }).Concat(buffer.Skip(ix + 1)).ToList() };
                }

                //// Param access -> lazy access.
                //if (paramAccess.Parameter.Returns == TangentType.Void) {
                //    throw new NotImplementedException("Attempting to work with a void parameter. wtf.");
                //} else {
                //    yield return new List<List<Expression>>(){ 
                //        buffer.Take(ix).Concat(new Expression[]{ 
                //            new FunctionBindingExpression(
                //                new ReductionDeclaration(
                //                    Enumerable.Empty<PhrasePart>(), 
                //                    new Function(
                //                        paramAccess.Parameter.Returns, 
                //                        new Block(
                //                            new[]{ 
                //                                 paramAccess }))), Enumerable.Empty<Expression>(), paramAccess.SourceInfo)}).ToList()};
                //}
            }

            //// Ctor param access -> lazy access
            //if (buffer[ix].NodeType == ExpressionNodeType.CtorParamAccess) {
            //    var ctorParamAccess = (CtorParameterAccessExpression)buffer[ix];
            //    yield return new List<List<Expression>>(){
            //        buffer.Take(ix).Concat(new Expression[]{ 
            //            new FunctionBindingExpression(
            //                new ReductionDeclaration(
            //                    Enumerable.Empty<PhrasePart>(), 
            //                    new Function(
            //                        ctorParamAccess.CtorParam.Returns,
            //                        new Block(
            //                            new[]{ 
            //                                 ctorParamAccess }))), Enumerable.Empty<Expression>(), ctorParamAccess.SourceInfo)}).ToList()};
            //}
        }

        public static bool IsMatch(List<Expression> input, List<PhrasePart> rule, Dictionary<ParameterDeclaration, TangentType> neededGenericInferences = null)
        {
            neededGenericInferences = neededGenericInferences ?? new Dictionary<ParameterDeclaration, TangentType>();
            if (input.Count < rule.Count) { return false; }
            var inputEnum = input.GetEnumerator();
            foreach (var entry in rule) {
                inputEnum.MoveNext();
                if (entry.IsIdentifier) {
                    if ((inputEnum.Current.NodeType != ExpressionNodeType.Identifier) ||
                        ((IdentifierExpression)inputEnum.Current).Identifier.Value != entry.Identifier.Value) {
                        return false;
                    }
                } else {
                    var inType = inputEnum.Current.EffectiveType;
                    if (inType == null) { return false; }
                    if (entry.Parameter.Returns == TangentType.Any.Kind && (inType.ImplementationType == KindOfType.Kind || inType.ImplementationType == KindOfType.TypeConstant || inType.ImplementationType == KindOfType.GenericReference || inType.ImplementationType == KindOfType.InferencePoint)) {
                        // good.
                    } else if (inType == TangentType.PotentiallyAnything) {
                        // good.
                    } else if (!entry.Parameter.Returns.CompatibilityMatches(inType, neededGenericInferences)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private List<ReductionDeclaration> PopBestCandidates(List<ReductionDeclaration> candidates)
        {
            var best = new List<ReductionDeclaration>();
            foreach (var entry in candidates) {
                if (!best.Any()) {
                    best.Add(entry);
                } else if (best.First().Takes.Count == entry.Takes.Count) {
                    var bestEnum = best.First().Takes.GetEnumerator();
                    var entryEnum = entry.Takes.GetEnumerator();
                    bool go = true;
                    while (go && bestEnum.MoveNext() && entryEnum.MoveNext()) {
                        int cmp = Compare(bestEnum.Current, entryEnum.Current);
                        if (cmp == -1) {
                            go = false;
                        } else if (cmp == 1) {
                            best.Clear();
                            best.Add(entry);
                            go = false;
                        }
                    }

                    if (go) {
                        best.Add(entry);
                    }
                } else {
                    candidates.RemoveAll(r => best.Contains(r));
                    return best;
                }
            }

            candidates.RemoveAll(r => best.Contains(r));
            return best;
        }

        private int Compare(dynamic a, dynamic b)
        {
            var phraseA = a is PhrasePart;
            var phraseB = b is PhrasePart;

            var idA = phraseA ? a.IsIdentifier : true;
            var idB = phraseB ? b.IsIdentifier : true;

            if (idA && !idB) {
                return -1;
            }

            if (idB && !idA) {
                return 1;
            }

            if (phraseA) {
                var ppa = ((PhrasePart)a);
                if (!ppa.IsIdentifier) {
                    if (ppa.Parameter.Returns.ImplementationType == KindOfType.SingleValue) {
                        if (phraseB) {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier && ppb.Parameter.Returns.ImplementationType == KindOfType.Enum) {
                                if (((SingleValueType)a.Parameter.Returns).ValueType == b.Parameter.Returns) {
                                    return -1;
                                }
                            }
                        }
                    } else if (ppa.Parameter.Returns.ImplementationType == KindOfType.Enum) {
                        if (phraseB) {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier && ppb.Parameter.Returns.ImplementationType == KindOfType.SingleValue) {
                                if (a.Parameter.Returns == ((SingleValueType)b.Parameter.Returns).ValueType) {
                                    return 1;
                                }
                            }
                        }
                    } else if (ppa.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference).Any()) {
                        if (phraseB) {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier){
                                if (!ppb.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference).Any()) {
                                    return 1;
                                }

                                var aCanInferB = ppa.Parameter.Returns.CompatibilityMatches(ppb.Parameter.Returns, new Dictionary<ParameterDeclaration, TangentType>());
                                var bCanInferA = ppb.Parameter.Returns.CompatibilityMatches(ppa.Parameter.Returns, new Dictionary<ParameterDeclaration, TangentType>());
                                if (aCanInferB) {
                                    if (bCanInferA) {
                                        // continue.
                                    } else {
                                        return 1;
                                    }
                                } else if (bCanInferA) {
                                    return -1;
                                }
                            }
                        }
                    } else {
                        if (phraseB) {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier && ppb.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference).Any()) {
                                return -1;
                            }
                        }
                    }
                }
            }

            return 0;
        }


        private IEnumerable<List<Expression>> PermutateParenBindings(List<PhrasePart> parameters, List<Expression> arguments, Dictionary<ParameterDeclaration, TangentType> typeInferences)
        {
            if (!arguments.Any(arg => arg.NodeType == ExpressionNodeType.ParenExpr)) { yield return arguments; yield break; }
            List<List<Expression>> potentials = arguments.Select((arg, ix) => arg.NodeType != ExpressionNodeType.ParenExpr ? new List<Expression>() { arg } : GetParenCandidates(parameters[ix], arg)).ToList();
            if (potentials.Any(p => !p.Any())) {
                // one of our parens cannot meet the requested param type.
                yield break;
            }

            // TODO: test this mess better.
            List<int> indexes = potentials.Select(p => 0).ToList();
            while (true) {
                yield return potentials.Select((p, ix) => p[indexes[ix]]).ToList();
                int i;
                for (i = 0; i < indexes.Count && indexes[i] == potentials[i].Count - 1; ++i) { }
                if (i == indexes.Count) { yield break; }
                indexes[i]++;
            }
        }

        private List<Expression> GetParenCandidates(PhrasePart phrasePart, Expression arg)
        {
            var parenExpr = arg as ParenExpression;
            return parenExpr.TryResolve(Scope, phrasePart.Parameter.Returns).ToList();
        }

        private FunctionInvocationExpression InferGenerics(ReductionDeclaration candidate, List<Expression> paramSet, LineColumnRange sourceInfo)
        {
            Dictionary<ParameterDeclaration, TangentType> inferences = new Dictionary<ParameterDeclaration, TangentType>();
            foreach (var pair in candidate.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).Zip(paramSet, (p, expr) => Tuple.Create(p, expr))) {
                Infer(pair.Item1.Returns, pair.Item2.EffectiveType, inferences);
            }

            if (!candidate.GenericParameters.All(p => inferences.ContainsKey(p))) { return null; }
            return new FunctionInvocationExpression(candidate, paramSet, candidate.GenericParameters.Select(p => inferences[p]).ToList(), sourceInfo);
        }

        private static void Infer(TangentType parameterType, TangentType effectiveType, Dictionary<ParameterDeclaration, TangentType> results)
        {
            if (parameterType.ImplementationType == KindOfType.InferencePoint) {
                GenericInferencePlaceholder placeholder = parameterType as GenericInferencePlaceholder;
                results.Add(placeholder.GenericArgument, effectiveType);
                return;
            }

            if (parameterType.ImplementationType == KindOfType.BoundGeneric && effectiveType.ImplementationType == KindOfType.BoundGeneric) {
                BoundGenericType boundParam = parameterType as BoundGenericType;
                BoundGenericType boundArg = effectiveType as BoundGenericType;
                if (boundParam.GenericTypeDeclatation == boundArg.GenericTypeDeclatation) {
                    foreach (var pair in boundParam.TypeArguments.Zip(boundArg.TypeArguments, (p, a) => Tuple.Create(p, a))) {
                        Infer(pair.Item1, pair.Item2, results);
                    }
                }
            }

            // else, not generic, no inference needed.
        }
    }
}
