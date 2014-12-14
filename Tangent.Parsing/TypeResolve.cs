using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing {
    public static class TypeResolve {
        public static ResultOrParseError<IEnumerable<ReductionDeclaration>> AllPartialFunctionDeclarations(IEnumerable<PartialReductionDeclaration> partialFunctions, IEnumerable<TypeDeclaration> types) {
            var errors = new List<BadTypePhrase>();
            var results = new List<ReductionDeclaration>();

            foreach (var fn in partialFunctions) {
                var resolutionResult = PartialFunctionDeclaration(fn, types);
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

        internal static ResultOrParseError<ReductionDeclaration> PartialFunctionDeclaration(PartialReductionDeclaration partialFunction, IEnumerable<TypeDeclaration> types) {
            var errors = new List<BadTypePhrase>();
            var phrase = new List<PhrasePart>();

            foreach (var part in partialFunction.Takes) {
                var resolved = Resolve(part, types);
                if (resolved.Success) {
                    phrase.Add(resolved.Result);
                } else {
                    errors.AddRange((resolved.Error as TypeResolutionErrors).Errors);
                }
            }

            var fn = partialFunction.Returns;
            var effectiveType = ResolveType(fn.EffectiveType, types);
            if (effectiveType == null) {
                errors.Add(new BadTypePhrase(fn.EffectiveType));
            }

            if (errors.Any()) {
                return new ResultOrParseError<ReductionDeclaration>(new TypeResolutionErrors(errors));
            }

            return new ResultOrParseError<ReductionDeclaration>(new ReductionDeclaration(phrase, new TypeResolvedFunction(effectiveType, fn.Implementation)));
        }

        internal static ResultOrParseError<PhrasePart> Resolve(PartialPhrasePart partial, IEnumerable<TypeDeclaration> types) {
            if (partial.IsIdentifier) {
                return new PhrasePart(partial.Identifier);
            }

            var resolved= Resolve(partial.Parameter, types);
            if (resolved.Success) {
                return new ResultOrParseError<PhrasePart>(new PhrasePart(resolved.Result));
            } else {
                return new ResultOrParseError<PhrasePart>(resolved.Error);
            }
        }

        internal static ResultOrParseError<ParameterDeclaration> Resolve(PartialParameterDeclaration partial, IEnumerable<TypeDeclaration> types) {
            var type = ResolveType(partial.Returns, types);
            if (type == null) {
                return new ResultOrParseError<ParameterDeclaration>(new TypeResolutionErrors(new[] { new BadTypePhrase(partial.Returns) }));
            }

            return new ResultOrParseError<ParameterDeclaration>(new ParameterDeclaration(partial.Takes, type));
        }

        internal static TangentType ResolveType(IEnumerable<Identifier> identifiers, IEnumerable<TypeDeclaration> types) {
            // TODO: fix perf.
            foreach (var type in types) {
                var typeIdentifiers = type.Takes;
                if (identifiers.SequenceEqual(typeIdentifiers)) {
                    return type.Returns;
                }
            }

            return null;
        }
    }
}
