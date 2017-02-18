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
        private readonly Dictionary<TangentType, TransformationLookupTree> DelegateParameterBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private readonly Dictionary<TangentType, List<TransformationLookupTree>> ConversionCache = new Dictionary<TangentType, List<TransformationLookupTree>>();
        private TransformationLookupTree AnyKindBranches = null;
        private readonly List<TransformationLookupTree> PrioritizedPotentiallyAnythingBranches = new List<TransformationLookupTree>();
        private readonly int PhraseIndex;
        private readonly ConversionGraph Conversions;

        public TransformationLookupTree(IEnumerable<TransformationRule> rules, ConversionGraph conversions) : this(rules, conversions, 0) { }

        private TransformationLookupTree(IEnumerable<TransformationRule> rules, ConversionGraph conversions, int depth)
        {
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
                        } else if (targetType == TangentType.PotentiallyAnything) {
                            foreach (var entry in PrioritizedPotentiallyAnythingBranches) {
                                foreach (var ruleset in entry.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }

                            foreach(var entry in DelegateParameterBranches.Where(kvp => !((DelegateType)kvp.Key).Takes.Any())) {
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
                            genericExprs.Add(expr);
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

            foreach (var entry in TransformationScopeOld.Prioritize(genericExprs)) {
                PrioritizedGenericBranches.Add(new TransformationLookupTree(entry, Conversions, index + 1));
            }

            foreach (var entry in TransformationScopeOld.Prioritize(ParameterRules.SelectMany(kvp => kvp.Value))) {
                PrioritizedPotentiallyAnythingBranches.Add(new TransformationLookupTree(entry, Conversions, index + 1));
            }
        }

        private void PopulateConversionCacheFor(TangentType target)
        {
            HashSet<TransformationRule> accumulatedRules = new HashSet<TransformationRule>();

            foreach (var branch in ParameterRules) {
                if (target != branch.Key) {
                    if (branch.Key.CompatibilityMatches(target, new Dictionary<ParameterDeclaration, TangentType>())) {
                        accumulatedRules.UnionWith(branch.Value);
                    }else {
                        var conversion = Conversions.FindConversion(target, branch.Key);
                        if (conversion != null) {
                            accumulatedRules.UnionWith(branch.Value);
                        }
                    }
                }
            }

            List<TransformationLookupTree> coersions = new List<TransformationLookupTree>();
            foreach(var entry in TransformationScopeOld.Prioritize(accumulatedRules)) {
                coersions.Add(new TransformationLookupTree(entry, Conversions, PhraseIndex + 1));
            }

            ConversionCache.Add(target, coersions);
        }
    }
}
