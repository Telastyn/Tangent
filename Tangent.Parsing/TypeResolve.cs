using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public static class TypeResolve
    {
        public static ResultOrParseError<IEnumerable<ReductionDeclaration>> AllPartialFunctionDeclarations(IEnumerable<PartialReductionDeclaration> partialFunctions, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new List<BadTypePhrase>();
            var results = new List<ReductionDeclaration>();

            foreach (var fn in partialFunctions) {
                var resolutionResult = PartialFunctionDeclaration(fn, types, conversions);
                if (resolutionResult.Success) {
                    results.Add(resolutionResult.Result);
                } else {
                    var typeIssues = resolutionResult.Error as TypeResolutionErrors;
                    errors.AddRange(typeIssues.Errors);
                }
            }

            if (errors.Any()) {
                return new ResultOrParseError<IEnumerable<ReductionDeclaration>>(new TypeResolutionErrors(errors));
            }

            return results;
        }

        public static ResultOrParseError<IEnumerable<TypeDeclaration>> AllTypePlaceholders(IEnumerable<TypeDeclaration> typeDecls, out Dictionary<TangentType, TangentType> placeholderConversions)
        {
            List<BadTypePhrase> errors = new List<BadTypePhrase>();
            Dictionary<TangentType, TangentType> inNeedOfPopulation = new Dictionary<TangentType, TangentType>();
            Func<TangentType, TangentType> selector = t => t;
            selector = t =>
            {
                if (t.ImplementationType == KindOfType.Sum) {
                    var newSum = SumType.For(((SumType)t).Types.Select(selector));
                    inNeedOfPopulation.Add(t, newSum);
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
                    var resolvedType = PartialProductType((PartialProductType)entry.Key, (ProductType)entry.Value, newLookup);
                    if (!resolvedType.Success) {
                        var resolutionErrors = (TypeResolutionErrors)resolvedType.Error;
                        errors.AddRange(resolutionErrors.Errors);
                    }
                } else if (entry.Key is PartialTypeReference) {
                    var reference = (PartialTypeReference)entry.Key;
                    references.Add(reference);
                    var resolvedType = ResolveType(reference.Identifiers, newLookup);
                    if (resolvedType.Success) {
                        reference.ResolvedType = resolvedType.Result;
                    } else {
                        var resolutionErrors = (TypeResolutionErrors)resolvedType.Error;
                        errors.AddRange(resolutionErrors.Errors);
                    }
                } else if (entry.Key is SumType) {
                    // Nothing, just need it in placeholder lists so that sum types with placeholders get fixed.
                } else {
                    throw new NotImplementedException();
                }
            }

            placeholderConversions = inNeedOfPopulation;

            if (errors.Any()) {
                return new ResultOrParseError<IEnumerable<TypeDeclaration>>(new TypeResolutionErrors(errors));
            }

            newLookup = newLookup.Select(td => new TypeDeclaration(td.Takes, selector(td.Returns))).ToList();

            return new ResultOrParseError<IEnumerable<TypeDeclaration>>(newLookup);
        }

        internal static ResultOrParseError<ReductionDeclaration> PartialFunctionDeclaration(PartialReductionDeclaration partialFunction, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new List<BadTypePhrase>();
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
                        errors.AddRange((resolved.Error as TypeResolutionErrors).Errors);
                    }
                }
            }

            var fn = partialFunction.Returns;
            var effectiveType = ResolveType(fn.EffectiveType, types);
            if (!effectiveType.Success) {
                errors.Add(new BadTypePhrase(fn.EffectiveType));
            }

            if (errors.Any()) {
                return new ResultOrParseError<ReductionDeclaration>(new TypeResolutionErrors(errors));
            }

            return new ResultOrParseError<ReductionDeclaration>(new ReductionDeclaration(phrase, new TypeResolvedFunction(effectiveType.Result, fn.Implementation, scope)));
        }

        internal static ResultOrParseError<ProductType> PartialProductType(PartialProductType partialType, ProductType target, IEnumerable<TypeDeclaration> types)
        {
            var errors = new List<BadTypePhrase>();

            foreach (var part in partialType.DataConstructorParts) {
                var resolved = Resolve(part, types);
                if (resolved.Success) {
                    target.DataConstructorParts.Add(resolved.Result);
                } else {
                    errors.AddRange((resolved.Error as TypeResolutionErrors).Errors);
                }
            }

            if (errors.Any()) {
                return new ResultOrParseError<ProductType>(new TypeResolutionErrors(errors));
            }

            return target;
        }

        internal static ResultOrParseError<PhrasePart> Resolve(PartialPhrasePart partial, IEnumerable<TypeDeclaration> types)
        {
            if (partial.IsIdentifier) {
                return new PhrasePart(partial.Identifier);
            }

            var resolved = Resolve(partial.Parameter, types);
            if (resolved.Success) {
                return new ResultOrParseError<PhrasePart>(new PhrasePart(resolved.Result));
            } else {
                return new ResultOrParseError<PhrasePart>(resolved.Error);
            }
        }

        internal static ResultOrParseError<ParameterDeclaration> Resolve(PartialParameterDeclaration partial, IEnumerable<TypeDeclaration> types)
        {
            var type = ResolveType(partial.Returns, types);
            if (!type.Success) {
                return new ResultOrParseError<ParameterDeclaration>(type.Error);
            }

            return new ParameterDeclaration(partial.Takes, type.Result);
        }

        internal static ResultOrParseError<TangentType> ResolveType(IEnumerable<Identifier> identifiers, IEnumerable<TypeDeclaration> types)
        {
            // TODO: fix perf.
            if (identifiers.First().Value == "~>") {
                var nested = ResolveType(identifiers.Skip(1), types);
                if (!nested.Success) {
                    return nested;
                }

                return nested.Result.Lazy;
            } else {
                List<int> dots = new List<int>();
                int ix = 0;
                foreach (var id in identifiers) {
                    if (id.Value == ".") {
                        dots.Add(ix);
                    }

                    ix++;
                }

                if (dots.Count == 1) {
                    // For now, values can only be a single identifier, so dots needs to be a b c.v
                    if (dots[0] != ix - 2) { return new ResultOrParseError<TangentType>(new TypeResolutionErrors(new[] { new BadTypePhrase(identifiers) })); }
                    var t = ResolveType(identifiers.Take(dots[0]), types);
                    if (!t.Success) { return t; }
                    if (t.Result.ImplementationType != KindOfType.Enum) { return new ResultOrParseError<TangentType>(new TypeResolutionErrors(new[] { new BadTypePhrase(identifiers) })); }
                    var tenum = t.Result as EnumType;
                    var v = identifiers.Last().Value;
                    if (!tenum.Values.Contains(v)) { return new ResultOrParseError<TangentType>(new TypeResolutionErrors(new[] { new BadTypePhrase(identifiers) })); }
                    return tenum.SingleValueTypeFor(v);
                } else if (dots.Count > 1) {
                    return new ResultOrParseError<TangentType>(new TypeResolutionErrors(new[] { new BadTypePhrase(identifiers) }));
                } // else

                foreach (var type in types) {
                    var typeIdentifiers = type.Takes;
                    if (identifiers.SequenceEqual(typeIdentifiers)) {
                        var reference = type.Returns as PartialTypeReference;
                        if (reference != null) {
                            return ResolvePlaceholderReference(reference, types);
                        }

                        var sum = type.Returns as SumType;
                        if (sum != null) {
                            bool replace = false;
                            List<TangentType> newbs = new List<TangentType>();
                            foreach (var t in sum.Types) {
                                var innerReference = t as PartialTypeReference;
                                if (innerReference != null) {
                                    replace = true;
                                    var innerResult = ResolvePlaceholderReference(innerReference, types);
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

                        return type.Returns;
                    }
                }

                return new ResultOrParseError<TangentType>(new TypeResolutionErrors(new[] { new BadTypePhrase(identifiers) }));
            }
        }

        private static ResultOrParseError<TangentType> ResolvePlaceholderReference(PartialTypeReference reference, IEnumerable<TypeDeclaration> types)
        {
            if (reference.ResolvedType == null) {
                var nested = ResolveType(reference.Identifiers, types);
                if (nested.Success) {
                    reference.ResolvedType = nested.Result;
                } else {
                    return new ResultOrParseError<TangentType>(nested.Error);
                }
            }

            return reference.ResolvedType;
        }
    }
}
