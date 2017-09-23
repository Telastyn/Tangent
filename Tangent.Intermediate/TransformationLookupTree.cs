using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TransformationLookupTree : ITransformationLookupTree
    {
        private readonly List<List<TransformationRule>> NonExpressionRules = new List<List<TransformationRule>>();
        private readonly List<List<TransformationRule>> ExpressionRules = new List<List<TransformationRule>>();
        private readonly Dictionary<Identifier, TransformationLookupTree> IdentifierBranches = new Dictionary<Identifier, TransformationLookupTree>();
        private readonly Dictionary<TangentType, TransformationLookupTree> SingleValueBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private readonly Dictionary<TangentType, TransformationLookupTree> ParameterMatchBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private readonly Dictionary<TangentType, List<TransformationRule>> ParameterRules = new Dictionary<TangentType, List<TransformationRule>>();
        private readonly List<TransformationLookupTree> PrioritizedGenericBranches = new List<TransformationLookupTree>();
        private readonly Dictionary<TangentType, List<TransformationLookupTree>> PrioritizedPartialGenericBranches = new Dictionary<TangentType, List<TransformationLookupTree>>();
        private readonly Dictionary<TangentType, TransformationLookupTree> DelegateParameterBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private readonly Dictionary<TangentType, List<TransformationLookupTree>> ConversionCache = new Dictionary<TangentType, List<TransformationLookupTree>>();
        private TransformationLookupTree AnyKindBranches = null;
        private readonly List<TransformationLookupTree> PrioritizedPotentiallyAnythingBranches = new List<TransformationLookupTree>();
        private readonly int PhraseIndex;
        private readonly ConversionGraph Conversions;

        public int ApproximateRulesetSize { get; private set; }

        public TransformationLookupTree(IEnumerable<TransformationRule> rules, ConversionGraph conversions) : this(rules, conversions, 0) { }

        private TransformationLookupTree(IEnumerable<TransformationRule> rules, ConversionGraph conversions, int depth)
        {
            ApproximateRulesetSize = rules.Count();
            Conversions = conversions;
            PhraseIndex = depth;
            if (rules.Any()) {
                BuildTree(rules, depth);
            }
        }

        public IEnumerable<IEnumerable<TransformationRule>> Lookup(IEnumerable<Expression> phrase)
        {
            var element = phrase.FirstOrDefault();
            if (element != null) {
                // return the longer matches from our children.
                if (element.NodeType == ExpressionNodeType.Identifier) {
                    var id = ((IdentifierExpression)element).Identifier;
                    if (IdentifierBranches.ContainsKey(id)) {
                        foreach (var entry in IdentifierBranches[id].Lookup(phrase.Skip(1))) {
                            yield return entry;
                        }
                    }
                } else {
                    var targetType = element.EffectiveType;
                    if (targetType != null) {
                        var targetBoundType = targetType as BoundGenericType;
                        var targetGenericType = targetBoundType?.GenericType;

                        if (targetType.ImplementationType == KindOfType.TypeConstant ||
                            targetType.ImplementationType == KindOfType.Kind ||
                            targetType.ImplementationType == KindOfType.GenericReference) {
                            if (AnyKindBranches != null) {
                                foreach (var entry in AnyKindBranches.Lookup(phrase.Skip(1))) {
                                    yield return entry;
                                }
                            }
                        }

                        if (element.NodeType == ExpressionNodeType.PartialLambda) {
                            foreach (var entry in DelegateParameterBranches) {
                                foreach (var ruleset in entry.Value.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                            // generics
                            if (targetGenericType != null && PrioritizedPartialGenericBranches.ContainsKey(targetGenericType)) {
                                foreach (var entry in PrioritizedPartialGenericBranches[targetGenericType]) {
                                    foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                        yield return ruleset;
                                    }
                                }
                            }

                            foreach (var entry in PrioritizedGenericBranches) {
                                foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }
                        } else if (targetType == TangentType.PotentiallyAnything) {
                            foreach (var entry in PrioritizedPotentiallyAnythingBranches) {
                                foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                            foreach (var entry in DelegateParameterBranches.Where(kvp => !((DelegateType)kvp.Key).Takes.Any())) {
                                foreach (var ruleset in entry.Value.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                        } else {
                            if (targetType.ImplementationType == KindOfType.SingleValue) {
                                if (SingleValueBranches.ContainsKey(targetType)) {
                                    foreach (var ruleset in SingleValueBranches[targetType].Lookup(phrase.Skip(1))) {
                                        yield return ruleset;
                                    }
                                }
                            }

                            if (DelegateParameterBranches.ContainsKey(targetType)) {
                                // A firm match, but for some reason I treated these special...
                                foreach(var entry in DelegateParameterBranches[targetType].Lookup(phrase.Skip(1))) {
                                    yield return entry;
                                }
                            }

                            if (DelegateParameterBranches.ContainsKey(targetType.Lazy)) {
                                // Find things that match  ~>target
                                foreach (var entry in DelegateParameterBranches[targetType.Lazy].Lookup(phrase.Skip(1))) {
                                    yield return entry;
                                }
                            }

                            // a firm match
                            if (ParameterMatchBranches.ContainsKey(targetType)) {
                                foreach (var ruleset in ParameterMatchBranches[targetType].Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                            // convertable match
                            if (!ConversionCache.ContainsKey(targetType)) {
                                PopulateConversionCacheFor(targetType);
                            }

                            foreach (var entry in ConversionCache[targetType]) {
                                foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                            // generics
                            if (targetGenericType != null) {
                                if (PrioritizedPartialGenericBranches.ContainsKey(targetGenericType)) {
                                    foreach (var entry in PrioritizedPartialGenericBranches[targetGenericType]) {
                                        foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                            yield return ruleset;
                                        }
                                    }
                                }

                                // TODO: cache? optimize?
                                var convertableTypes = Conversions.CandidateTypesFor(targetType);

                                List<Tuple<ConversionPath, TransformationLookupTree>> candidates = new List<Tuple<ConversionPath, TransformationLookupTree>>();
                                foreach (var parameterCandidate in convertableTypes.Where(tt => tt.ImplementationType != KindOfType.BoundGeneric)) {
                                    if (ParameterMatchBranches.ContainsKey(parameterCandidate)) {
                                        var conversion = Conversions.FindConversion(targetType, parameterCandidate);
                                        candidates.Add(Tuple.Create(conversion, ParameterMatchBranches[parameterCandidate]));
                                    }
                                }

                                foreach (var group in candidates.GroupBy(c => c.Item1.Cost).OrderBy(g => g.Key)) {
                                    var tier = group.ToList();
                                    if (tier.Count > 1) {
                                        throw new NotImplementedException("Ambiguous partial generic conversions not yet supported.");
                                    }

                                    foreach (var ruleset in tier.First().Item2.Lookup(phrase.Skip(1))) {
                                        yield return ruleset;
                                    }
                                }

                                var partialCandidates = convertableTypes.Where(tt => tt.ImplementationType == KindOfType.BoundGeneric).Cast<BoundGenericType>().Where(bgt => PrioritizedPartialGenericBranches.ContainsKey(bgt.GenericType)).Select(bgt => bgt.GenericType).ToList();
                                if (partialCandidates.Count > 1) {
                                    throw new NotImplementedException("Ambiguous partial generic conversions not yet supported.");
                                }

                                foreach (var entry in partialCandidates) {
                                    foreach (var tier in PrioritizedPartialGenericBranches[entry]) {
                                        foreach (var ruleset in tier.Lookup(phrase.Skip(1))) {
                                            yield return ruleset;
                                        }
                                    }
                                }
                            }

                            foreach (var entry in PrioritizedGenericBranches) {
                                foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }
                        }
                    }
                }
            } // else we've checked the entire phrase. Return what is here.

            foreach (var entry in NonExpressionRules) {
                yield return entry;
            }

            foreach (var entry in ExpressionRules) {
                yield return entry;
            }
        }

        private void BuildTree(IEnumerable<TransformationRule> rules, int index)
        {
            List<TransformationRule> nonExprs = new List<TransformationRule>();
            List<TransformationRule> exprs = new List<TransformationRule>();
            List<TransformationRule> anyKindRules = new List<TransformationRule>();
            Dictionary<Identifier, List<TransformationRule>> identifierRules = new Dictionary<Identifier, List<TransformationRule>>();
            Dictionary<TangentType, List<TransformationRule>> delegateRules = new Dictionary<TangentType, List<TransformationRule>>();
            Dictionary<TangentType, List<TransformationRule>> singleValueRules = new Dictionary<TangentType, List<TransformationRule>>();
            Dictionary<TangentType, List<ExpressionDeclaration>> partialGenericRules = new Dictionary<TangentType, List<ExpressionDeclaration>>();
            List<ExpressionDeclaration> genericExprs = new List<ExpressionDeclaration>();

            foreach (var rule in rules) {
                var expr = rule as ExpressionDeclaration;
                if (expr == null) {
                    nonExprs.Add(rule);
                } else {
                    var pp = expr.DeclaredPhrase.Pattern.Skip(index).FirstOrDefault();
                    if (pp == null) {
                        exprs.Add(rule);
                    } else {
                        if (pp.IsIdentifier) {
                            if (!identifierRules.ContainsKey(pp.Identifier)) {
                                identifierRules.Add(pp.Identifier, new List<TransformationRule>());
                            }

                            identifierRules[pp.Identifier].Add(rule);
                        } else if (pp.Parameter.RequiredArgumentType == TangentType.Any.Kind) {
                            anyKindRules.Add(rule);

                            // RMS: the second condition here is necessary for the Ternary tests to work, but seems to break other things. Take care.
                        } else if (pp.Parameter.RequiredArgumentType.ImplementationType == KindOfType.Delegate && !pp.Parameter.RequiredArgumentType.ContainedGenericReferences().Any()) {
                            if (!delegateRules.ContainsKey(pp.Parameter.RequiredArgumentType)) {
                                delegateRules.Add(pp.Parameter.RequiredArgumentType, new List<TransformationRule>());
                            }

                            delegateRules[pp.Parameter.RequiredArgumentType].Add(rule);
                        } else if (pp.Parameter.RequiredArgumentType.ImplementationType == KindOfType.SingleValue) {
                            if (!singleValueRules.ContainsKey(pp.Parameter.RequiredArgumentType)) {
                                singleValueRules.Add(pp.Parameter.RequiredArgumentType, new List<TransformationRule>());
                            }

                            singleValueRules[pp.Parameter.RequiredArgumentType].Add(rule);
                        } else if (pp.Parameter.RequiredArgumentType.ContainedGenericReferences().Any()) {
                            var bgt = pp.Parameter.RequiredArgumentType as BoundGenericType;
                            if (bgt != null) {
                                if (!partialGenericRules.ContainsKey(bgt.GenericType)) {
                                    partialGenericRules.Add(bgt.GenericType, new List<ExpressionDeclaration>());
                                }

                                partialGenericRules[bgt.GenericType].Add(expr);
                            } else {
                                genericExprs.Add(expr);
                            }
                        } else {
                            if (!ParameterRules.ContainsKey(pp.Parameter.RequiredArgumentType)) {
                                ParameterRules.Add(pp.Parameter.RequiredArgumentType, new List<TransformationRule>());
                            }

                            ParameterRules[pp.Parameter.RequiredArgumentType].Add(rule);
                        }
                    }
                }
            }

            if (nonExprs.Any()) {
                foreach (var tier in TransformationScopeOld.Prioritize(nonExprs)) {
                    NonExpressionRules.Add(new List<TransformationRule>(tier));
                }
            }

            if (exprs.Any()) {
                foreach (var tier in TransformationScopeOld.Prioritize(exprs)) {
                    ExpressionRules.Add(new List<TransformationRule>(tier));
                }
            }

            if (anyKindRules.Any()) {
                AnyKindBranches = new TransformationLookupTree(anyKindRules, Conversions, index + 1);
            }

            foreach (var entry in identifierRules) {
                IdentifierBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, Conversions, index + 1));
            }

            foreach (var entry in delegateRules) {
                DelegateParameterBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, Conversions, index + 1));
            }

            foreach (var entry in ParameterRules) {
                ParameterMatchBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, Conversions, index + 1));
            }

            foreach (var entry in singleValueRules) {
                SingleValueBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, Conversions, index + 1));
            }

            foreach (var entry in partialGenericRules) {
                PrioritizedPartialGenericBranches.Add(entry.Key, new List<TransformationLookupTree>());
                foreach (var tier in TransformationScopeOld.Prioritize(entry.Value)) {
                    PrioritizedPartialGenericBranches[entry.Key].Add(new TransformationLookupTree(tier, Conversions, index + 1));
                }
            }

            foreach (var entry in TransformationScopeOld.Prioritize(genericExprs)) {
                PrioritizedGenericBranches.Add(new TransformationLookupTree(entry, Conversions, index + 1));
            }

            foreach (var entry in TransformationScopeOld.Prioritize(ParameterRules.SelectMany(kvp => kvp.Value))) {
                PrioritizedPotentiallyAnythingBranches.Add(new TransformationLookupTree(entry, Conversions, index + 1));
            }

            // RMS: 7/22/17 - Had this here to make sure that the conversion graph was being used for lazy things, but seems unnecessary...
            //foreach (var entry in delegateRules) {
            //    ParameterRules.Add(entry.Key, entry.Value);
            //}
        }

        private void PopulateConversionCacheFor(TangentType target)
        {
            HashSet<TransformationRule> accumulatedRules = new HashSet<TransformationRule>();

            foreach (var branch in ParameterRules) {
                if (target != branch.Key) {
                    if (branch.Key.CompatibilityMatches(target, new Dictionary<ParameterDeclaration, TangentType>())) {
                        accumulatedRules.UnionWith(branch.Value);
                    } else {
                        var conversion = Conversions.FindConversion(target, branch.Key);
                        if (conversion != null) {
                            accumulatedRules.UnionWith(branch.Value);
                        }
                    }
                }
            }

            List<TransformationLookupTree> coersions = new List<TransformationLookupTree>();
            foreach (var entry in TransformationScopeOld.Prioritize(accumulatedRules)) {
                coersions.Add(new TransformationLookupTree(entry, Conversions, PhraseIndex + 1));
            }

            ConversionCache.Add(target, coersions);
        }
    }
}
