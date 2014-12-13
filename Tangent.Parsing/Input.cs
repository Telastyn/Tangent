using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing {
    public class Input {
        private readonly List<Expression> buffer;
        public readonly Scope Scope;
        private readonly List<TypeResolvedReductionDeclaration> conversionsTaken;

        public Input(IEnumerable<Identifier> identifiers, Scope scope) {
            buffer = new List<Expression>(identifiers.Select(id => new IdentifierExpression(id)));
            Scope = scope;
            conversionsTaken = new List<TypeResolvedReductionDeclaration>();
        }

        private Input(IEnumerable<Expression> exprs, Scope scope, IEnumerable<TypeResolvedReductionDeclaration> conversionsTaken) {
            buffer = new List<Expression>(exprs);
            Scope = scope;
            this.conversionsTaken = new List<TypeResolvedReductionDeclaration>(conversionsTaken);
        }

        public List<Expression> InterpretAsExpression() {
            return InterpretTowards(TangentType.Void, true).Select(r => r.First()).ToList();
        }

        private List<List<Expression>> InterpretTowards(TangentType type, bool requireFullMatch) {
            List<List<Expression>> result = new List<List<Expression>>();
            switch (buffer.First().NodeType) {
                case ExpressionNodeType.ParameterAccess:
                    var firstParam = buffer.First() as ParameterAccessExpression;
                    if (firstParam.Parameter.Returns == type) {
                        if (requireFullMatch) {
                            if (buffer.Count == 1) {
                                result.Add(buffer);
                                return result;
                            }
                        } else {
                            result.Add(buffer);
                        }
                    }

                    // TODO: reduction of parameter if rule takes non-terminals.
                    foreach (var candidateSet in CandidatesInPriorityOrder()) {
                        foreach (var candidateEntry in candidateSet) {
                            // Candidates will match the first token, no matter what.
                            List<List<Expression>> bindings = new List<List<Expression>>() { new List<Expression>() { buffer.First() } };
                            List<Expression> workingSet = new List<Expression>(buffer.Skip(1));
                            for (int ix = 1; bindings != null && ix < candidateEntry.Takes.Count; ++ix) {
                                PhrasePart paramTake = Fix(candidateEntry.Takes[ix]);
                                if (paramTake.IsIdentifier) {
                                    // Easy part. Check for ID. Match is good.
                                    if ((workingSet[0] as IdentifierExpression).Identifier.Value == paramTake.Identifier) {
                                        workingSet.RemoveAt(0);
                                    } else {
                                        // mismatch. Exit.
                                        bindings = null;
                                    }
                                } else {
                                    // Hard part. Find something that matches our type.
                                    if (ix == candidateEntry.Takes.Count - 1) {
                                        // We're the last param. Match all.
                                        var paramResult = new Input(workingSet, Scope, conversionsTaken).InterpretTowards(paramTake.Parameter.Returns, true);
                                        if(!paramResult.Any()){
                                            // failure.
                                            bindings = null;
                                        }else{
                                            bindings.AddRange(paramResult);
                                        }
                                    }else{
                                        PhrasePart nextParam = Fix(candidateEntry.Takes[ix+1]);
                                        if(nextParam.IsIdentifier){
                                            // Cool, a terminal.
                                            foreach(var potentialSubstring in GenerateSubstrings(workingSet, nextParam.Identifier)){

                                            }
                                        }else{
                                        }
                                }
                            }
                        }

                        if (result.Any()) {
                            return result;
                        }
                    }
                    break;
                case ExpressionNodeType.FunctionInvocation:
                    var firstFn = buffer.First() as FunctionInvocationExpression;
                    if (firstFn.EffectiveType == type) {
                        if (requireFullMatch) {
                            if (buffer.Count == 1) {
                                result.Add(buffer);
                                return result;
                            }
                        } else {
                            result.Add(buffer);
                        }
                    }

                    // TODO: reduction of function if rule takes non-terminals.
                    break;
                case ExpressionNodeType.TypeAccess:
                    throw new NotImplementedException();
                case ExpressionNodeType.FunctionBinding:
                    // No rules currently support ~>T.
                    return new Input(new[] {new FunctionInvocationExpression(
                        buffer.First() as FunctionBindingExpression)}.Concat(buffer.Skip(1)).ToList(), Scope, conversionsTaken).InterpretTowards(type, requireFullMatch);
                case ExpressionNodeType.Identifier:
                    // TODO: try reductions
                    break;
            }

            throw new NotImplementedException("Shouldn't get here.");
        }

        private PhrasePart Fix(dynamic param) {
            if (param is PhrasePart) {
                return param;
            }

            if (param is Identifier) {
                return new PhrasePart(param);
            }

            throw new InvalidOperationException();
        }

        private IEnumerable<List<ReductionRule<dynamic, dynamic>>> CandidatesInPriorityOrder() {
            // Parameters first.
            var parameterCandidates = Scope.Parameters.Where(pd => HasTerminalsInOrder(pd.Takes.Select(id => new PhrasePart(id)))).Select(c => new ReductionRule<dynamic, dynamic>(c.Takes, c.Returns)).ToList();
            while (parameterCandidates.Any()) {
                yield return PopBestCandidates(parameterCandidates);
            }

            // Then type constants.
            var typeCandidates = Scope.Types.Where(td => HasTerminalsInOrder(td.Takes.Select(id => new PhrasePart(id)))).Select(c => new ReductionRule<dynamic, dynamic>(c.Takes, c.Returns)).ToList();
            while (typeCandidates.Any()) {
                yield return PopBestCandidates(typeCandidates);
            }

            // Then functions.
            var functionCandidates = Scope.Functions.Where(fd => !conversionsTaken.Contains(fd) && HasTerminalsInOrder(fd.Takes)).Select(c => new ReductionRule<dynamic, dynamic>(c.Takes, c.Returns)).ToList();
            while (functionCandidates.Any()) {
                yield return PopBestCandidates(functionCandidates);
            }
        }

        private bool HasTerminalsInOrder(IEnumerable<PhrasePart> reductionTerminals) {
            var terminalEnumerator = reductionTerminals.GetEnumerator();
            var bufferEnumerator = buffer.GetEnumerator();
            bufferEnumerator.MoveNext();
            terminalEnumerator.MoveNext();
            bool canSkip = false;
            bool first = true;
            while (true) {
                if (terminalEnumerator.Current.IsIdentifier) {
                    if (bufferEnumerator.Current is IdentifierExpression && ((IdentifierExpression)bufferEnumerator.Current).Identifier.Value != terminalEnumerator.Current.Identifier.Value) {
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
                    }
                } else {
                    if (first) {
                        // On first token, we need to match the rule's parameter type explicitly
                        if (bufferEnumerator.Current.NodeType == ExpressionNodeType.ParameterAccess &&
                            ((ParameterAccessExpression)bufferEnumerator.Current).Parameter.Returns != terminalEnumerator.Current.Parameter.Returns) {
                            return false;
                        }

                        if (bufferEnumerator.Current.NodeType == ExpressionNodeType.FunctionInvocation &&
                            ((FunctionInvocationExpression)bufferEnumerator.Current).EffectiveType != terminalEnumerator.Current.Parameter.Returns) {
                            return false;
                        }
                    }

                    if (!bufferEnumerator.MoveNext()) {
                        // We need some non-terminal, but there's no tokens.
                        return false;
                    }

                    canSkip = true;
                    // and loop.

                    if (!terminalEnumerator.MoveNext()) {
                        // We need some non-terminal at the end of the reduction rule, and there's
                        //  at least one token there. Assume that token satisfies our need.
                        return true;
                    }
                }

                first = false;
            }
        }


        private List<ReductionRule<dynamic, dynamic>> PopBestCandidates(List<ReductionRule<dynamic, dynamic>> candidates) {
            var best = new List<ReductionRule<dynamic, dynamic>>();
            foreach (var entry in candidates) {
                if (!best.Any() || best.First().Takes.Count == entry.Takes.Count) {
                    best.Add(entry);
                } else {
                    int cmp = Compare(best.First().Takes.First(), entry.Takes.First());
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
                }
            }

            candidates.RemoveAll(r => best.Contains(r));
            return best;
        }

        private int Compare(dynamic a, dynamic b) {
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
