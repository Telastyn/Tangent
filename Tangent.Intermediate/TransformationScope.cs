using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TransformationScope
    {
        public readonly IEnumerable<IEnumerable<TransformationRule>> Rules;

        public TransformationScope(IEnumerable<TransformationRule> rules)
        {
            Rules = Prioritize(rules).ToList();
        }

        public List<Expression> InterpretStatement(List<Expression> input)
        {
            return InterpretTowards(TangentType.Void, input);
        }

        public List<Expression> InterpretTowards(TangentType target, List<Expression> input)
        {
            return InterpretTowards(target, input, Enumerable.Empty<List<Expression>>());
        }

        private List<Expression> InterpretTowards(TangentType target, List<Expression> input, IEnumerable<List<Expression>> history)
        {
            // TODO: optimize; History can be cleared on non-conversion, and this only needs checked on conversion.
            if (history.Any(entry => entry.Count == input.Count && entry.Zip(input, (i, e) => new { inputEntry = i, historyEntry = e }).All(pair => pair.inputEntry.EffectiveType == pair.historyEntry.EffectiveType && pair.inputEntry.NodeType == pair.historyEntry.NodeType))) {
                return new List<Expression>();
            }

            if (input.Count == 1) {
                if (target == input[0].EffectiveType) {
                    return input;
                } else if (target == TangentType.Any.Kind && (input[0].EffectiveType is KindType || input[0].EffectiveType is TypeConstant || input[0].EffectiveType is GenericArgumentReferenceType || input[0].EffectiveType is GenericInferencePlaceholder)) {
                    // mild hack because there is no subtyping.
                    return input;
                }
            }

            for (int ix = 0; ix < input.Count; ++ix) {
                var buffer = input.GetRange(ix, input.Count - ix);
                foreach (var tier in Rules) {
                    var reductions = tier.Select(r => r.TryReduce(buffer, this)).Where(r => r.Success).ToList();

                    // TODO: toss cycles?
                    var successes = reductions.SelectMany(r => InterpretTowards(target, input.Take(ix).Concat(new[] { r.ReplacesWith }).Concat(buffer.Skip(r.Takes)).ToList(), history.Concat(new[] { buffer }))).ToList();
                    if (successes.Any()) {
                        return successes;
                    }
                }
            }

            return new List<Expression>();
        }

        public static IEnumerable<IEnumerable<TransformationRule>> Prioritize(IEnumerable<TransformationRule> rules)
        {
            var groups = rules.GroupBy(r => r.Type).OrderBy(g => g.Key);
            foreach (var group in groups) {
                var tiers = group.GroupBy(r => r.MaxTakeCount).OrderByDescending(g => g.Key);
                foreach (var tier in tiers) {
                    var exprGroups = tier.GroupBy(r => r is ExpressionDeclaration);
                    var nonExprs = exprGroups.FirstOrDefault(g => g.Key == false);
                    if (nonExprs != null) {
                        yield return nonExprs;
                    }

                    var exprs = exprGroups.FirstOrDefault(g => g.Key == true);
                    if (exprs != null) {
                        var exprList = exprs.Cast<ExpressionDeclaration>().ToList();
                        while (exprList.Any()) {
                            yield return PopBestCandidates(exprList);
                        }
                    }
                }
            }
        }

        private static List<ExpressionDeclaration> PopBestCandidates(List<ExpressionDeclaration> rules)
        {
            var best = new List<ExpressionDeclaration>();
            foreach (var entry in rules) {
                if (!best.Any()) {
                    best.Add(entry);
                } else {

                    var cmp = PhrasePriorityComparer.ComparePriority(best.First().DeclaredPhrase, entry.DeclaredPhrase);
                    if (cmp == 0) {
                        best.Add(entry);
                    } else if (cmp > 0) {
                        best.Clear();
                        best.Add(entry);
                    }
                }
            }

            rules.RemoveAll(r => best.Contains(r));
            return best;
        }
    }
}
