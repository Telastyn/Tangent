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
        private readonly List<ReductionDeclaration> conversionsTaken;

        public Input(IEnumerable<Identifier> identifiers, Scope scope)
        {
            buffer = new List<Expression>(identifiers.Select(id => new IdentifierExpression(id)));
            Scope = scope;
            conversionsTaken = new List<ReductionDeclaration>();
        }

        public Input(IEnumerable<Expression> exprs, Scope scope) : this(exprs, scope, new List<ReductionDeclaration>()) { }

        private Input(IEnumerable<Expression> exprs, Scope scope, IEnumerable<ReductionDeclaration> conversionsTaken)
        {
            buffer = new List<Expression>(exprs);
            Scope = scope;
            this.conversionsTaken = new List<ReductionDeclaration>(conversionsTaken);
        }

        public List<Expression> InterpretAsStatement()
        {
            return InterpretTowards(TangentType.Void);
        }

        private List<Expression> InterpretTowards(TangentType type)
        {
            if (buffer.Count == 1) {
                if (type == GetEffectiveTypeIfPossible(buffer[0])) {
                    return buffer;
                }
            }

            for (int ix = 0; ix < buffer.Count; ++ix) {
                var inProgressBinding = buffer[ix] as HalfBoundExpression;
                if (inProgressBinding != null) {
                    if (inProgressBinding.IsDone) {
                        // We've reached the end of some binding. Tie it off and step. We don't care what the final results are here since we have no alternatives.
                        return new Input(buffer.Take(ix).Concat(new[] { inProgressBinding.FullyBind() }).Concat(buffer.Skip(ix + 1)), Scope, conversionsTaken).InterpretTowards(type);
                    }

                    if (ix == buffer.Count - 1) {
                        // We have an empty binding, but no more tokens. Parse failure.
                        return new List<Expression>();
                    }

                    var nextToken = inProgressBinding.NextStep;
                    if (nextToken.IsIdentifier) {
                        if (buffer[ix + 1] is IdentifierExpression && ((IdentifierExpression)buffer[ix + 1]).Identifier.Value == nextToken.Identifier.Value) {
                            // Identifier to consume.
                            inProgressBinding.Bindings.Add(buffer[ix + 1]);
                            return new Input(buffer.Take(ix + 1).Concat(buffer.Skip(ix + 2)), Scope, conversionsTaken).InterpretTowards(type);
                        } else {
                            // No match, and no reduction will create one.
                            return new List<Expression>();
                        }
                    } else {
                        if (nextToken.Parameter.Returns == GetEffectiveTypeIfPossible(buffer[ix + 1])) {
                            // Value to consume.
                            inProgressBinding.Bindings.Add(buffer[ix + 1]);

                            // And recurse.
                            var result = new Input(buffer.Take(ix + 1).Concat(buffer.Skip(ix + 2)), Scope, conversionsTaken).InterpretTowards(type);
                            if (result.Any()) {
                                return result;
                            } else {
                                // We could bind the next token, but that does not lead to success...
                                //  continue on, and hope it gets reduced further.
                            }
                        } else {
                            // We need some T that isn't there. Continue and hope it gets reduced to what we need.
                        }
                    }
                }

                // Otherwise, we have some elemental token here. Try candidates (in order) to see if it can be reduced.
                foreach (var candidateSet in CandidatesInPriorityOrder(buffer.Skip(ix))) {
                    List<Expression> successes = new List<Expression>();
                    foreach (var entry in candidateSet) {
                        // Bind the first param to the rule and recurse.
                        entry.Bindings.Add(buffer[ix]);
                        successes.AddRange(new Input(buffer.Take(ix).Concat(new[] { entry }).Concat(buffer.Skip(ix + 1)), Scope, conversionsTaken).InterpretTowards(type));
                    }

                    if (successes.Any()) {
                        return successes;
                    }
                }

                // If we have a bound function, and nothing can use it simply bound, try invoking it.
                var binding = buffer[ix] as FunctionBindingExpression;
                if (binding != null) {
                    var result = new Input(buffer.Take(ix).Concat(new[] { new FunctionInvocationExpression(binding) }).Concat(buffer.Skip(ix + 1)), Scope, conversionsTaken).InterpretTowards(type);
                    if (result.Any()) {
                        return result;
                    }
                }

                // If we have a parameter that is a bound function, and nothing can use it simply bound, try invoking it.
                var lazyParam = buffer[ix] as ParameterAccessExpression;
                if (lazyParam != null) {
                    if (lazyParam.Parameter.Returns.ImplementationType == KindOfType.Lazy) {
                        var result = new Input(buffer.Take(ix).Concat(new[] { new DelegateInvocationExpression(lazyParam) }).Concat(buffer.Skip(ix + 1)), Scope, conversionsTaken).InterpretTowards(type);
                        if (result.Any()) {
                            return result;
                        }
                    }
                }

                // No luck. Go to next token and see if reducing there helps.
            }

            // And if we've gotten here, we have some bundle of tokens that cannot be reduced further but don't result in our target.
            return new List<Expression>();
        }

        private TangentType GetEffectiveTypeIfPossible(Expression expr)
        {
            switch (expr.NodeType) {
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    return invoke.EffectiveType;

                case ExpressionNodeType.ParameterAccess:
                    var param = (ParameterAccessExpression)expr;
                    return param.Parameter.Returns;

                case ExpressionNodeType.FunctionBinding:
                    var binding = expr as FunctionBindingExpression;
                    return binding.EffectiveType;

                case ExpressionNodeType.HalfBoundExpression:
                    var halfBinding = expr as HalfBoundExpression;
                    if (!halfBinding.IsDone) {
                        return null;
                    }

                    return halfBinding.EffectiveType;

                case ExpressionNodeType.DelegateInvocation:
                    var delegateInvoke = expr as DelegateInvocationExpression;
                    return delegateInvoke.EffectiveType;

                case ExpressionNodeType.Constant:
                    var constant = expr as ConstantExpression;
                    return constant.EffectiveType;

                case ExpressionNodeType.TypeAccess:
                case ExpressionNodeType.Identifier:
                case ExpressionNodeType.Unknown:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<List<HalfBoundExpression>> CandidatesInPriorityOrder(IEnumerable<Expression> tokenStream)
        {
            // Parameters first.
            var parameterCandidates = Scope.Parameters.Where(pd => HasTerminalsInOrder(pd.Takes.Select(id => new PhrasePart(id)), tokenStream)).Select(c => new HalfBoundExpression(c)).ToList();
            while (parameterCandidates.Any()) {
                yield return PopBestCandidates(parameterCandidates);
            }

            // Then type constants.
            var typeCandidates = Scope.Types.Where(td => HasTerminalsInOrder(td.Takes.Select(id => new PhrasePart(id)), tokenStream)).Select(c => new HalfBoundExpression(c)).ToList();
            while (typeCandidates.Any()) {
                yield return PopBestCandidates(typeCandidates);
            }

            // Then functions.
            var functionCandidates = Scope.Functions.Where(fd => !conversionsTaken.Contains(fd) && HasTerminalsInOrder(fd.Takes, tokenStream)).Select(c => new HalfBoundExpression(c)).ToList();
            while (functionCandidates.Any()) {
                yield return PopBestCandidates(functionCandidates);
            }
        }

        private bool HasTerminalsInOrder(IEnumerable<PhrasePart> reductionTerminals, IEnumerable<Expression> tokenStream)
        {
            var terminalEnumerator = reductionTerminals.GetEnumerator();
            var bufferEnumerator = tokenStream.GetEnumerator();
            bufferEnumerator.MoveNext();
            terminalEnumerator.MoveNext();
            bool canSkip = false;
            bool first = true;
            while (true) {
                if (terminalEnumerator.Current.IsIdentifier) {
                    if (!(bufferEnumerator.Current is IdentifierExpression) || ((IdentifierExpression)bufferEnumerator.Current).Identifier.Value != terminalEnumerator.Current.Identifier.Value) {
                        if (canSkip) {
                            if (!bufferEnumerator.MoveNext()) {
                                return false;
                            } else {
                                // loop.
                            }
                        } else {
                            // We need some non-terminal, but did not find it.
                            return false;
                        }

                    } else {
                        if (!terminalEnumerator.MoveNext()) {
                            // No more reductions? Cool.
                            return true;
                        }

                        if (!bufferEnumerator.MoveNext()) {
                            // There's more reduction rules, but no tokens.
                            return false;
                        }

                        canSkip = false;
                    }
                } else {
                    if (first) {

                        switch (bufferEnumerator.Current.NodeType) {
                            case ExpressionNodeType.ParameterAccess:
                                if (((ParameterAccessExpression)bufferEnumerator.Current).Parameter.Returns != terminalEnumerator.Current.Parameter.Returns) {
                                    return false;
                                }

                                // else good.
                                break;

                            case ExpressionNodeType.FunctionInvocation:
                                if (((FunctionInvocationExpression)bufferEnumerator.Current).EffectiveType != terminalEnumerator.Current.Parameter.Returns) {
                                    return false;
                                }

                                // else good.
                                break;

                            case ExpressionNodeType.DelegateInvocation:
                                if (((DelegateInvocationExpression)bufferEnumerator.Current).EffectiveType != terminalEnumerator.Current.Parameter.Returns) {
                                    return false;
                                }
                                // else good.
                                break;

                            case ExpressionNodeType.Constant:
                                if (((ConstantExpression)bufferEnumerator.Current).EffectiveType != terminalEnumerator.Current.Parameter.Returns) {
                                    return false;
                                }
                                // else good.
                                break;

                            case ExpressionNodeType.HalfBoundExpression:
                            case ExpressionNodeType.FunctionBinding:
                            case ExpressionNodeType.Identifier:
                            case ExpressionNodeType.TypeAccess:
                                return false;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    canSkip = true;
                    // and loop.

                    if (!terminalEnumerator.MoveNext()) {
                        // We need some non-terminal at the end of the reduction rule, and there's
                        //  at least one token there. Assume that token satisfies our need.
                        return true;
                    }

                    if (!bufferEnumerator.MoveNext()) {
                        // We've got a rule, but no tokens. Fail.
                        return false;
                    }
                }

                first = false;
            }
        }


        private List<HalfBoundExpression> PopBestCandidates(List<HalfBoundExpression> candidates)
        {
            var best = new List<HalfBoundExpression>();
            foreach (var entry in candidates) {
                if (!best.Any()) {
                    best.Add(entry);
                } else if (best.First().Rule.Takes.Count == entry.Rule.Takes.Count) {
                    int cmp = Compare(best.First().Rule.Takes.First(), entry.Rule.Takes.First());
                    switch (cmp) {
                        case -1:
                            // Best is better. Leave best alone.
                            break;
                        case 0:
                            // Equal priority.
                            best.Add(entry);
                            break;
                        case 1:
                            // entry is better.
                            best.Clear();
                            best.Add(entry);
                            break;
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

            return 0;
        }
    }
}
