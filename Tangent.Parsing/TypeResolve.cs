using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Parsing.Partial;
using Tangent.Parsing.Transformations;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public static class TypeResolve
    {
        private static readonly IEnumerable<TransformationRule> typeResolutionRules = new TransformationRule[] { LazyOperator.Common, SingleValueAccessor.Common };

        public static ResultOrParseError<IEnumerable<TypeDeclaration>> AllPartialTypeDeclarations(IEnumerable<PartialTypeDeclaration> partialTypes, IEnumerable<TypeDeclaration> builtInTypes, out Dictionary<PartialParameterDeclaration, ParameterDeclaration> genericArgumentMapping)
        {
            List<TypeDeclaration> types = new List<TypeDeclaration>(builtInTypes);
            var simpleTypes = partialTypes.Where(ptd => ptd.Takes.All(pp => pp.IsIdentifier));
            types.AddRange(simpleTypes.Select(ptd => new TypeDeclaration(ptd.Takes.Select(ppp => new PhrasePart(ppp.Identifier)), ptd.Returns)));
            var leftToProcess = partialTypes.Except(simpleTypes).ToList();
            genericArgumentMapping = new Dictionary<PartialParameterDeclaration, ParameterDeclaration>();

            while (leftToProcess.Any()) {
                List<PartialTypeDeclaration> removals = new List<PartialTypeDeclaration>();

                foreach (var entry in leftToProcess) {
                    var resolution = TryPartialTypeDeclaration(entry, types, false);
                    if (resolution != null) {
                        if (!resolution.Success) {
                            return new ResultOrParseError<IEnumerable<TypeDeclaration>>(resolution.Error);
                        }

                        removals.Add(entry);
                        types.Add(resolution.Result);
                    }
                }

                if (removals.Any()) {
                    leftToProcess = leftToProcess.Except(removals).ToList();
                } else {
                    return new ResultOrParseError<IEnumerable<TypeDeclaration>>(new AggregateParseError(leftToProcess.SelectMany(ptd => ptd.Takes.Where(ppp => !ppp.IsIdentifier).Select(ppp => new IncomprehensibleStatementError(ppp.Parameter.Returns)))));
                }
            }

            // Unfortunately, what we resolved mid-way into building types might not be the same thing we resolve now that we have all the types.
            // Even though it is costly, we will double check and toss if things no longer parse unambiguously.
            var genericTypes = partialTypes.Except(simpleTypes).Select(pt => Tuple.Create(pt, TryPartialTypeDeclaration(pt, types, true))).ToList();
            var issues = genericTypes.Where(i => !i.Item2.Success).Select(i => i.Item2).ToList();
            if (issues.Any()) {
                return new ResultOrParseError<IEnumerable<TypeDeclaration>>(new AggregateParseError(issues.Select(i => i.Error)));
            }

            foreach (var entry in genericTypes) {
                foreach (var phrasePartPair in entry.Item1.Takes.Zip(entry.Item2.Result.Takes, (ppp, pp) => Tuple.Create(ppp, pp))) {
                    if (!phrasePartPair.Item1.IsIdentifier) {
                        genericArgumentMapping.Add(phrasePartPair.Item1.Parameter, phrasePartPair.Item2.Parameter);
                    }
                }
            }

            return types;
        }

        public static ResultOrParseError<TypeDeclaration> TryPartialTypeDeclaration(PartialTypeDeclaration partial, IEnumerable<TypeDeclaration> types, bool hardError)
        {
            var scope = Scope.ForTypes(types, Enumerable.Empty<ParameterDeclaration>());
            List<PhrasePart> takes = new List<PhrasePart>();
            foreach (var t in partial.Takes) {
                if (t.IsIdentifier) {
                    takes.Add(new PhrasePart(t.Identifier));
                } else {
                    var input = new Input(t.Parameter.Returns, scope);
                    var interpretResults = input.InterpretTowards(TangentType.Any.Kind);
                    if (interpretResults.Count == 1) {
                        takes.Add(new PhrasePart(new ParameterDeclaration(t.Parameter.Takes, interpretResults.Cast<TypeAccessExpression>().First().TypeConstant.Value.Kind)));
                    } else if (interpretResults.Count == 0) {
                        // No way to parse things. 
                        if (!hardError) {
                            return null;
                        } else {
                            return new ResultOrParseError<TypeDeclaration>(new IncomprehensibleStatementError(t.Parameter.Returns));
                        }
                    } else {
                        return new ResultOrParseError<TypeDeclaration>(new AmbiguousStatementError(t.Parameter.Returns, interpretResults));
                    }
                }
            }

            return new TypeDeclaration(takes, partial.Returns);
        }

        public static ResultOrParseError<IEnumerable<ReductionDeclaration>> AllPartialFunctionDeclarations(IEnumerable<PartialReductionDeclaration> partialFunctions, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            var results = new List<ReductionDeclaration>();

            foreach (var fn in partialFunctions) {
                var resolutionResult = PartialFunctionDeclaration(fn, types, conversions);
                if (resolutionResult.Success) {
                    results.Add(resolutionResult.Result);
                } else {
                    errors = errors.Concat(resolutionResult.Error);
                }
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<IEnumerable<ReductionDeclaration>>(errors);
            }

            return results;
        }

        public static ResultOrParseError<IEnumerable<TypeDeclaration>> AllTypePlaceholders(IEnumerable<TypeDeclaration> typeDecls, Dictionary<PartialParameterDeclaration, ParameterDeclaration> genericArgumentMapping, out Dictionary<TangentType, TangentType> placeholderConversions)
        {
            AggregateParseError errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            Dictionary<TangentType, TangentType> inNeedOfPopulation = new Dictionary<TangentType, TangentType>();
            Func<TangentType, TangentType> selector = t => t;
            selector = t =>
            {
                if (t.ImplementationType == KindOfType.Sum) {
                    var newSum = SumType.For(((SumType)t).Types.Select(selector));
                    if (!inNeedOfPopulation.ContainsKey(t)) {
                        inNeedOfPopulation.Add(t, newSum);
                    } else {
                        inNeedOfPopulation[t] = newSum;
                    }
                    return newSum;
                } else if (t is PartialProductType) {
                    var newb = new ProductType(Enumerable.Empty<PhrasePart>());
                    inNeedOfPopulation.Add((PartialProductType)t, newb);
                    return newb;
                } else if (t is PartialTypeReference) {
                    var reference = (PartialTypeReference)t;
                    var target = reference.ResolvedType == null ? reference : reference.ResolvedType;
                    if (!inNeedOfPopulation.ContainsKey(reference)) {
                        inNeedOfPopulation.Add(reference, target);
                    } else {
                        inNeedOfPopulation[reference] = target;
                    }

                    return target;
                } else {
                    return t;
                }
            };

            var newLookup = typeDecls.Select(td => new TypeDeclaration(td.Takes, selector(td.Returns))).ToList();
            var references = new HashSet<PartialTypeReference>();

            foreach (var entry in inNeedOfPopulation) {
                if (entry.Key is PartialProductType) {
                    var ppt = (PartialProductType)entry.Key;
                    var resolvedType = PartialProductType(ppt, (ProductType)entry.Value, newLookup, ppt.GenericArguments.Select(ppd => genericArgumentMapping[ppd]));
                    if (!resolvedType.Success) {
                        errors = errors.Concat(resolvedType.Error);
                    }
                } else if (entry.Key is PartialTypeReference) {
                    var reference = (PartialTypeReference)entry.Key;
                    references.Add(reference);
                    var resolvedType = ResolveType(reference.Identifiers, newLookup, reference.GenericArgumentPlaceholders.Select(ppd => genericArgumentMapping[ppd]));
                    if (resolvedType.Success) {
                        reference.ResolvedType = resolvedType.Result;
                    } else {
                        errors = errors.Concat(resolvedType.Error);
                    }
                } else if (entry.Key is SumType) {
                    // Nothing, just need it in placeholder lists so that sum types with placeholders get fixed.
                } else {
                    throw new NotImplementedException();
                }
            }

            placeholderConversions = inNeedOfPopulation;

            if (errors.Errors.Any()) {
                return new ResultOrParseError<IEnumerable<TypeDeclaration>>(errors);
            }

            newLookup = newLookup.Select(td => new TypeDeclaration(td.Takes, selector(td.Returns))).ToList();

            return new ResultOrParseError<IEnumerable<TypeDeclaration>>(newLookup);
        }

        internal static ResultOrParseError<ReductionDeclaration> PartialFunctionDeclaration(PartialReductionDeclaration partialFunction, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            var phrase = new List<PhrasePart>();
            bool thisFound = false;

            ProductType scope = null;
            if (partialFunction.Returns.Scope != null) {
                scope = (ProductType)conversions[partialFunction.Returns.Scope];
            }

            foreach (var part in partialFunction.Takes) {
                if (!part.IsIdentifier && part.Parameter.IsThisParam) {
                    if (thisFound) { // TODO: nicer error.
                        throw new ApplicationException("Multiple this parameters declared in function.");
                    }

                    phrase.Add(new PhrasePart(new ParameterDeclaration("this", scope)));
                    thisFound = true;
                } else {
                    var resolved = Resolve(part, types);
                    if (resolved.Success) {
                        phrase.Add(resolved.Result);
                    } else {
                        errors = errors.Concat(resolved.Error);
                    }
                }
            }

            var fn = partialFunction.Returns;
            var effectiveType = ResolveType(fn.EffectiveType, types, Enumerable.Empty<ParameterDeclaration>());
            if (!effectiveType.Success) {
                errors = errors.Concat(effectiveType.Error);
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<ReductionDeclaration>(errors);
            }

            return new ResultOrParseError<ReductionDeclaration>(new ReductionDeclaration(phrase, new TypeResolvedFunction(effectiveType.Result, fn.Implementation, scope)));
        }

        internal static ResultOrParseError<ProductType> PartialProductType(PartialProductType partialType, ProductType target, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());

            foreach (var part in partialType.DataConstructorParts) {
                var resolved = Resolve(part, types, genericArguments);
                if (resolved.Success) {
                    target.DataConstructorParts.Add(resolved.Result);
                } else {
                    errors = errors.Concat(resolved.Error);
                }
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<ProductType>(errors);
            }

            return target;
        }

        internal static ResultOrParseError<PhrasePart> Resolve(PartialPhrasePart partial, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> ctorGenericArguments = null)
        {
            if (partial.IsIdentifier) {
                return new PhrasePart(partial.Identifier);
            }

            var resolved = Resolve(partial.Parameter, types, ctorGenericArguments);
            if (resolved.Success) {
                return new ResultOrParseError<PhrasePart>(new PhrasePart(resolved.Result));
            } else {
                return new ResultOrParseError<PhrasePart>(resolved.Error);
            }
        }

        internal static ResultOrParseError<ParameterDeclaration> Resolve(PartialParameterDeclaration partial, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> ctorGenericArguments = null)
        {
            var type = ResolveType(partial.Returns, types, ctorGenericArguments ?? Enumerable.Empty<ParameterDeclaration>());
            if (!type.Success) {
                return new ResultOrParseError<ParameterDeclaration>(type.Error);
            }

            return new ParameterDeclaration(partial.Takes, ctorGenericArguments == null ? type.Result : ConvertGenericReferencesToInferences(type.Result));
        }

        internal static ResultOrParseError<TangentType> ResolveType(IEnumerable<IdentifierExpression> identifiers, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            var input = new Input(identifiers, Scope.ForTypes(types, genericArguments), typeResolutionRules);
            var result = input.InterpretTowards(TangentType.Any.Kind);
            if (result.Count == 1) {
                var resolvedType = result[0].EffectiveType;

                if (resolvedType.ImplementationType == KindOfType.TypeConstant) {
                    resolvedType = ((TypeConstant)resolvedType).Value;

                    var reference = resolvedType as PartialTypeReference;
                    if (reference != null) {
                        return ResolvePlaceholderReference(reference, types, genericArguments);
                    }

                    var sum = resolvedType as SumType;
                    if (sum != null) {
                        bool replace = false;
                        List<TangentType> newbs = new List<TangentType>();
                        foreach (var t in sum.Types) {
                            var innerReference = t as PartialTypeReference;
                            if (innerReference != null) {
                                replace = true;
                                var innerResult = ResolvePlaceholderReference(innerReference, types, genericArguments);
                                if (innerResult.Success) {
                                    newbs.Add(innerResult.Result);
                                } else {
                                    return innerResult;
                                }
                            } else {
                                newbs.Add(t);
                            }
                        }

                        if (replace) {
                            return SumType.For(newbs);
                        } else {
                            return sum;
                        }
                    }

                    return resolvedType;
                } else if (resolvedType.ImplementationType == KindOfType.GenericReference) {
                    // some type reference. Just go with it?
                    return resolvedType;
                } else {
                    throw new NotImplementedException();
                }
            }

            if (result.Count == 0) {
                return new ResultOrParseError<TangentType>(new IncomprehensibleStatementError(identifiers));
            } else {
                return new ResultOrParseError<TangentType>(new AmbiguousStatementError(identifiers, result));
            }
        }

        private static ResultOrParseError<TangentType> ResolvePlaceholderReference(PartialTypeReference reference, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            if (reference.ResolvedType == null) {
                var nested = ResolveType(reference.Identifiers, types, genericArguments);
                if (nested.Success) {
                    reference.ResolvedType = nested.Result;
                } else {
                    return new ResultOrParseError<TangentType>(nested.Error);
                }
            }

            return reference.ResolvedType;
        }

        private static TangentType ConvertGenericReferencesToInferences(TangentType input)
        {
            switch (input.ImplementationType) {
                case KindOfType.BoundGeneric:
                    var boundGeneric = input as BoundGenericType;
                    return BoundGenericType.For(boundGeneric.GenericTypeDeclatation, boundGeneric.TypeArguments.Select(ta => ConvertGenericReferencesToInferences(ta)));
                case KindOfType.Builtin:
                case KindOfType.Enum:
                case KindOfType.SingleValue:
                case KindOfType.InferencePoint:
                case KindOfType.Placeholder:
                case KindOfType.Product:
                case KindOfType.Sum:
                    return input;
                case KindOfType.GenericReference:
                    var genref = input as GenericArgumentReferenceType;
                    return GenericInferencePlaceholder.For(genref.GenericParameter);
                case KindOfType.Kind:
                    var kind = input as KindType;
                    return ConvertGenericReferencesToInferences(kind.KindOf).Kind;
                case KindOfType.Lazy:
                    var lazy = input as LazyType;
                    return ConvertGenericReferencesToInferences(lazy.Type).Lazy;
                default:
                    throw new NotImplementedException();

            }
        }
    }
}
