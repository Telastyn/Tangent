using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing {
    public static class InterpretExpression {
        public static Expression ForStatement(IEnumerable<Identifier> tokens, Scope scope) {
            var result = ForType(TangentType.Void, tokens.Select(id => (Expression)new IdentifierExpression(id)).ToList(), scope, true);
            if (result != null) {
                return result.First();
            }

            return null;
        }

        public static List<Expression> ForType(TangentType target, List<Expression> tokens, Scope scope, bool mustComplete) {
            var invoke = tokens.First() as FunctionInvocationExpression;
            if (invoke != null) {

                // If we've found a result, return it
                if (invoke.EffectiveType == target) {

                    if (mustComplete && tokens.Skip(1).Any()) {
                    } else {
                        return tokens;
                    }
                }

                foreach (var functionCandidate in scope.Functions) {
                    var result = TryBindFunction(functionCandidate, tokens, scope);
                    if (result != null) {
                        return ForType(target, result, scope, mustComplete);
                    }
                }

                return null;
            }

            var id = tokens.First() as IdentifierExpression;
            if (id != null) {

                foreach (var parameterCandidate in scope.Parameters) {
                    var takeParts = parameterCandidate.TakeParts().Select(i => i.Value).ToList();

                    if (takeParts.SequenceEqual(tokens.Cast<IdentifierExpression>().Select(i => i.Identifier.Value))) {
                        var newb = new[] { new ParameterAccessExpression(parameterCandidate) }.Concat(tokens.Skip(parameterCandidate.TakeParts().Count()).ToList()).ToList();
                        var result = ForType(target, newb, scope, mustComplete);
                        if (result != null) {
                            return result;
                        }
                    }

                }

                foreach (var typeCandidate in scope.Types) {
                    var takeParts = typeCandidate.TakeParts().Select(i => i.Value).ToList();
                    if (takeParts.SequenceEqual(tokens.Cast<IdentifierExpression>().Select(i => i.Identifier.Value))) {
                        var newb = new[] { new TypeAccessExpression(typeCandidate.EndResult()) }.Concat(tokens.Skip(typeCandidate.TakeParts().Count()).ToList()).ToList();
                        var result = ForType(target, newb, scope, mustComplete);
                        if (result != null) {
                            return result;
                        }
                    }
                }

                foreach (var functionCandidate in scope.Functions) {
                    var result = TryBindFunction(functionCandidate, tokens, scope);
                    if (result != null) {
                        return ForType(target, result, scope, mustComplete);
                    }
                }
            }

            var binding = tokens.First() as FunctionBindingExpression;
            if (binding != null) {
                // For now, we don't have lazy types, so check it immediately.
                var result = ForType(target, new[] { new FunctionInvocationExpression(binding) }.Concat(tokens.Skip(1).ToList()).ToList(), scope, mustComplete);
                if (result != null) {
                    return result;
                }
            }

            var typeAccess = tokens.First() as TypeAccessExpression;
            if (typeAccess != null) {
                throw new NotImplementedException();
            }

            var parameter = tokens.First() as ParameterAccessExpression;
            if (parameter != null) {
                if (parameter.Parameter.EndResult() == target) {
                    if (mustComplete && tokens.Skip(1).Any()) {
                    } else {
                        return tokens;
                    }
                }

                foreach (var functionCandidate in scope.Functions) {
                    var result = TryBindFunction(functionCandidate, tokens, scope);
                    if (result != null) {
                        return ForType(target, result, scope, mustComplete);
                    }
                }
            }

            throw new NotImplementedException();
        }

        private static List<Expression> TryBindFunction(TypeResolvedReductionDeclaration function, List<Expression> tokens, Scope scope) {
            List<Expression> buffer = new List<Expression>(tokens);
            List<Expression> boundParameters = new List<Expression>();
            foreach (var phrasePart in function.TakeParts().ToList()) {
                if (!buffer.Any()) { return null; }
                if (phrasePart.IsIdentifier) {
                    var id = buffer.First() as IdentifierExpression;
                    if (id == null) {
                        return null;
                    }

                    if (id.Identifier.Value != phrasePart.Identifier.Value) {
                        return null;
                    }

                    buffer.RemoveAt(0);
                } else {
                    var result = ForType(phrasePart.Parameter.EndResult(), buffer, scope, false);
                    if (result == null) {
                        return null;
                    }

                    boundParameters.Add(result.First());
                    buffer = result.Skip(1).ToList();
                }
            }

            return new[] { new FunctionBindingExpression(new ReductionDeclaration(function.Takes, function.EndResult()), boundParameters) }.Concat(buffer).ToList();
        }
    }
}
