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
        private readonly Dictionary<TangentType, TransformationLookupTree> ParameterBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private readonly Dictionary<TangentType, TransformationLookupTree> DelegateParameterBranches = new Dictionary<TangentType, TransformationLookupTree>();
        private TransformationLookupTree AnyKindBranches = null;

        public TransformationLookupTree(IEnumerable<TransformationRule> rules) : this(rules, 0) { }

        private TransformationLookupTree(IEnumerable<TransformationRule> rules, int depth)
        {
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
                            targetType.ImplementationType == KindOfType.GenericReference ||
                            targetType.ImplementationType == KindOfType.InferencePoint) {
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
                            foreach (var entry in ParameterBranches.Concat(DelegateParameterBranches)) {
                                foreach (var ruleset in entry.Value.Lookup(phrase.Skip(1))) {
                                    yield return ruleset;
                                }
                            }
                        } else {
                            if (DelegateParameterBranches.ContainsKey(targetType.Lazy)) {
                                // Find things that match  ~>target
                                foreach (var entry in DelegateParameterBranches[targetType.Lazy].Lookup(phrase.Skip(1))) {
                                    yield return entry;
                                }
                            }

                            // matches, compatibilities, and conversions. For now, just check them all.
                            throw new NotImplementedException("This needs fixed. Parameter Branches need prioritized like PhrasePriorityComparer.");
                            foreach (var entry in ParameterBranches) {
                                foreach (var ruleset in entry.Value.Lookup(phrase.Skip(1))) {
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
            Dictionary<TangentType, List<TransformationRule>> parameterRules = new Dictionary<TangentType, List<TransformationRule>>();
            Dictionary<TangentType, List<TransformationRule>> delegateRules = new Dictionary<TangentType, List<TransformationRule>>();

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
                        } else if (pp.Parameter.RequiredArgumentType.ImplementationType == KindOfType.Delegate) {
                            if (!delegateRules.ContainsKey(pp.Parameter.RequiredArgumentType)) {
                                delegateRules.Add(pp.Parameter.RequiredArgumentType, new List<TransformationRule>());
                            }

                            delegateRules[pp.Parameter.RequiredArgumentType].Add(rule);
                        } else {
                            if (!parameterRules.ContainsKey(pp.Parameter.RequiredArgumentType)) {
                                parameterRules.Add(pp.Parameter.RequiredArgumentType, new List<TransformationRule>());
                            }

                            parameterRules[pp.Parameter.RequiredArgumentType].Add(rule);
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
                AnyKindBranches = new TransformationLookupTree(anyKindRules, index + 1);
            }

            foreach (var entry in identifierRules) {
                IdentifierBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, index + 1));
            }

            foreach (var entry in delegateRules) {
                DelegateParameterBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, index + 1));
            }

            foreach (var entry in parameterRules) {
                ParameterBranches.Add(entry.Key, new TransformationLookupTree(entry.Value, index + 1));
            }
        }
    }
}
