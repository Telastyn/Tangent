using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TransformationScopeNew : TransformationScope
    {
        public ConversionGraph Conversions { get; private set; }
        public int ApproximateRulesetSize { get; private set; }
        private readonly ITransformationLookupTree LookupTree;

        public TransformationScopeNew(IEnumerable<TransformationRule> rules, ConversionGraph conversions)
        {
            Conversions = conversions;
            LookupTree = new TransformationLookupTree(rules, conversions);
            ApproximateRulesetSize = LookupTree.ApproximateRulesetSize + conversions.ApproximateRulesetSize;
        }

        private TransformationScopeNew(ITransformationLookupTree tree, ConversionGraph conversions)
        {
            LookupTree = tree;
            Conversions = conversions;
            ApproximateRulesetSize = tree.ApproximateRulesetSize + conversions.ApproximateRulesetSize;
        }

        public List<Expression> InterpretTowards(TangentType target, List<Expression> input)
        {
            //System.IO.File.AppendAllText("h:\\tangent-trace.txt", string.Join("|", input.Select(x => x.ToString())) + "\n");
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
                foreach (var tier in LookupTree.Lookup(buffer)) {
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


        public TransformationScope CreateNestedLocalScope(IEnumerable<ParameterDeclaration> locals)
        {
            if (!locals.Any()) {
                return this;
            }

            return new TransformationScopeNew(
                new CompositeTransformationLookupTree(
                    new TransformationLookupTree(locals.SelectMany(l => LocalAccess.RulesForLocal(l)), Conversions),
                    LookupTree
                ), Conversions);
        }

        public TransformationScope CreateNestedParameterScope(IEnumerable<ParameterDeclaration> parameters)
        {
            if (!parameters.Any()) {
                return this;
            }

            return new TransformationScopeNew(
                new CompositeTransformationLookupTree(
                    new TransformationLookupTree(parameters.Select(p => new ParameterAccess(p)), Conversions),
                    LookupTree
                ), Conversions);
        }


        private IEnumerable<IEnumerable<TransformationResult>> OrderMatches(List<TransformationResult> reductions)
        {
            if (reductions.Count == 1) { yield return reductions; yield break; }
            while (reductions.Any()) {
                yield return PopBestCandidates(reductions);
            }
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
