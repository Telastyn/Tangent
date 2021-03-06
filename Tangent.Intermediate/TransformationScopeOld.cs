﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TransformationScopeOld : TransformationScope
    {
        public readonly IEnumerable<IEnumerable<TransformationRule>> Rules;
        public ConversionGraph Conversions { get; private set; }
        public int ApproximateRulesetSize { get; private set; }

        public TransformationScopeOld(IEnumerable<TransformationRule> rules, ConversionGraph conversionGraph)
        {
            Rules = Prioritize(rules).ToList();
            Conversions = conversionGraph;
            ApproximateRulesetSize = rules.Count() + conversionGraph.ApproximateRulesetSize;
        }

        private TransformationScopeOld(TransformationScopeOld parent, IEnumerable<ParameterDeclaration> nestedLocals)
        {
            Rules = new[] { nestedLocals.SelectMany(l => LocalAccess.RulesForLocal(l)).ToList() }.Concat(parent.Rules);
            Conversions = parent.Conversions;
            ApproximateRulesetSize = Rules.Count() + Conversions.ApproximateRulesetSize;
        }

        public List<Expression> InterpretTowards(TangentType target, List<Expression> input)
        {
            //System.IO.File.AppendAllText("h:\\guessing.txt", string.Join("|", input.Select(x => x.ToString())) + "\n");
            if (input.Count == 1) {
                if (target == input[0].EffectiveType) {
                    return input;
                } else if (target == TangentType.Any.Kind && (input[0].EffectiveType is KindType || input[0].EffectiveType is TypeConstant || input[0].EffectiveType is GenericArgumentReferenceType)) {
                    // mild hack because there is no subtyping.
                    return input;
                } else if (target.ImplementationType == KindOfType.Delegate && input[0].NodeType == ExpressionNodeType.PartialLambda) {
                    var lambda = ((PartialLambdaExpression)input[0]).TryToFitIn(target);
                    if (lambda != null) {
                        return new List<Expression>() { lambda };
                    }
                } else if (input[0].NodeType != ExpressionNodeType.Identifier) {
                    var conversionPath = Conversions.FindConversion(input[0].EffectiveType, target);
                    if (conversionPath != null) {
                        var expr = conversionPath.Convert(input[0], this);
                        var ambiguousConversion = expr as AmbiguousExpression;
                        if (ambiguousConversion != null) {
                            return ambiguousConversion.PossibleInterpretations.ToList();
                        }

                        return new List<Expression>() { expr };
                    }
                }
            }

            for (int ix = 0; ix < input.Count; ++ix) {
                var buffer = input.GetRange(ix, input.Count - ix);
                foreach (var tier in Rules) {
                    var reductions = tier.Select(r => r.TryReduce(buffer, this)).Where(r => r.Success).ToList();
                    foreach (var preferences in OrderMatches(reductions)) {

                        var successes = preferences.SelectMany(r => InterpretTowards(target, input.Take(ix).Concat(new[] { r.ReplacesWith }).Concat(buffer.Skip(r.Takes)).ToList())).ToList();
                        if (successes.Any()) {
                            return successes;
                        }
                    }
                }
            }

            return new List<Expression>();
        }

        private IEnumerable<IEnumerable<TransformationResult>> OrderMatches(List<TransformationResult> reductions)
        {
            if (reductions.Count == 1) { yield return reductions; yield break; }
            while (reductions.Any()) {
                yield return PopBestCandidates(reductions);
            }
        }

        public TransformationScope CreateNestedLocalScope(IEnumerable<ParameterDeclaration> locals)
        {
            if (!locals.Any()) {
                return this;
            }

            return new TransformationScopeOld(this, locals);
        }

        public TransformationScope CreateNestedParameterScope(IEnumerable<ParameterDeclaration> parameters)
        {
            if (!parameters.Any()) {
                return this;
            }

            return new TransformationScopeOld(Rules.SelectMany(x => x).Concat(parameters.Select(pd => new ParameterAccess(pd))), Conversions);
        }

        public static IEnumerable<IEnumerable<TransformationRule>> Prioritize(IEnumerable<TransformationRule> rules)
        {
            //var newb = NewPrioritize(rules).ToList();
            //var old = OldPrioritize(rules).ToList();

            ////Console.WriteLine(newb);
            ////Console.WriteLine(old);

            //return newb;
            return NewPrioritize(rules);
        }

        private static IEnumerable<IEnumerable<TransformationRule>> NewPrioritize(IEnumerable<TransformationRule> rules)
        {
            if (!rules.Any()) { yield break; }

            var groups = rules.GroupBy(r => r.Type).OrderBy(g => g.Key);
            foreach (var group in groups) {
                var tiers = group.GroupBy(r => r.MaxTakeCount).OrderByDescending(g => g.Key);
                foreach (var tier in tiers) {
                    var nonExprs = new List<TransformationRule>();
                    var exprs = new List<ExpressionDeclaration>();
                    foreach (var entry in tier) {
                        var expr = entry as ExpressionDeclaration;
                        if (expr == null) {
                            nonExprs.Add(entry);
                        } else {
                            exprs.Add(expr);
                        }
                    }

                    if (nonExprs.Any()) {
                        yield return nonExprs;
                    }

                    if (exprs.Any()) {
                        foreach (var partition in SortAndPartitionSortedCandidates(exprs)) {
                            yield return partition;
                        }
                    }
                }
            }
        }

        private static IEnumerable<IEnumerable<TransformationRule>> OldPrioritize(IEnumerable<TransformationRule> rules)
        {
            if (!rules.Any()) { yield break; }

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

        private static IEnumerable<IEnumerable<ExpressionDeclaration>> SortAndPartitionSortedCandidates(List<ExpressionDeclaration> rules)
        {
            rules.Sort(new ExpressionDeclarationPriorityComparer().Reverse());

            int skip = 0;
            for (int ix = 1; ix < rules.Count; ++ix) {
                if (0 != PhrasePriorityComparer.Common.Compare(rules[ix - 1].DeclaredPhrase, rules[ix].DeclaredPhrase)) {
                    yield return rules.GetRange(skip, ix - skip);
                    skip = ix;
                }
            }

            yield return rules.GetRange(skip, rules.Count - skip);
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

        private static List<TransformationResult> PopBestCandidates(List<TransformationResult> reductions)
        {
            var best = new List<TransformationResult>();
            foreach (var entry in reductions) {
                if (!best.Any()) {
                    best.Add(entry);
                } else {
                    var cmp = ResultPriorityComparer.ComparePriority(best.First(), entry);
                    if (cmp == 0) {
                        best.Add(entry);
                    } else if (cmp > 0) {
                        best.Clear();
                        best.Add(entry);
                    }
                }
            }

            reductions.RemoveAll(r => best.Contains(r));
            return best;
        }
    }
}
