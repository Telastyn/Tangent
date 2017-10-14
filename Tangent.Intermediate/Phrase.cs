using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class Phrase
    {
        public readonly IEnumerable<PhrasePart> Pattern;

        public Phrase(IEnumerable<PhrasePart> pattern)
        {
            Pattern = new List<PhrasePart>(pattern);
        }

        public Phrase(params string[] identifiers) : this(identifiers.Select(id => new PhrasePart(new Identifier(id)))) { }

        public Phrase ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return new Phrase(Pattern.Select(pp => pp.ResolveGenericReferences(mapping)));
        }

        public PhraseMatchResult TryMatch(IEnumerable<Expression> input, TransformationScope scope)
        {
            var inferenceCollector = new Dictionary<ParameterDeclaration, TangentType>();
            var parameterCollector = new List<Expression>();
            var conversionCollector = new List<ConversionPath>();
            var sourceInfoCollector = new List<LineColumnRange>();
            var lazyLambdaMatches = new List<Tuple<FitableLambda, TangentType>>();
            var inputEnum = input.GetEnumerator();
            foreach (var element in Pattern) {
                if (!inputEnum.MoveNext()) { return PhraseMatchResult.Failure; }
                if (element.IsIdentifier) {
                    if ((inputEnum.Current.NodeType != ExpressionNodeType.Identifier) ||
                        ((IdentifierExpression)inputEnum.Current).Identifier.Value != element.Identifier.Value) {
                        return PhraseMatchResult.Failure;
                    } else {
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    }
                } else {
                    var inType = inputEnum.Current.EffectiveType;
                    if (inType == null) { return PhraseMatchResult.Failure; }
                    if (element.Parameter.RequiredArgumentType == TangentType.Any.Kind && (inType.ImplementationType == KindOfType.Kind || inType.ImplementationType == KindOfType.TypeConstant || inType.ImplementationType == KindOfType.GenericReference)) {
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                        if (inType.ImplementationType == KindOfType.TypeConstant) {
                            if (inferenceCollector.ContainsKey(element.Parameter)) {
                                if (inferenceCollector[element.Parameter] != ((TypeConstant)inType).Value) {
                                    return PhraseMatchResult.Failure;
                                }
                            } else {
                                inferenceCollector.Add(element.Parameter, ((TypeConstant)inType).Value);
                            }
                        } else if (inType.ImplementationType == KindOfType.Kind) {
                            // Some generic access
                            if (inferenceCollector.ContainsKey(element.Parameter)) {
                                if (inferenceCollector[element.Parameter] != ((KindType)inType).KindOf) {
                                    return PhraseMatchResult.Failure;
                                }
                            } else {
                                inferenceCollector.Add(element.Parameter, ((KindType)inType).KindOf);
                            }
                        } else {
                            if (inferenceCollector.ContainsKey(element.Parameter)) {
                                if (inferenceCollector[element.Parameter] != inType) {
                                    return PhraseMatchResult.Failure;
                                }
                            } else {
                                inferenceCollector.Add(element.Parameter, inType);
                            }
                        }
                    } else if (inputEnum.Current.NodeType == ExpressionNodeType.PartialLambda || inputEnum.Current.NodeType == ExpressionNodeType.PartialLambdaGroup) {
                        if (element.Parameter.RequiredArgumentType.ContainedGenericReferences().Any()) {
                            // Lambdas can't deal with generics. Infer via the rest of the phrase and try at the end.
                            lazyLambdaMatches.Add(Tuple.Create((FitableLambda)inputEnum.Current, element.Parameter.RequiredArgumentType));
                            parameterCollector.Add(inputEnum.Current);
                            conversionCollector.Add(null);
                            sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                        } else {
                            var lambda = ((FitableLambda)inputEnum.Current).TryToFitIn(element.Parameter.RequiredArgumentType);
                            if (lambda == null) {
                                return PhraseMatchResult.Failure;
                            }

                            parameterCollector.Add(lambda);
                            conversionCollector.Add(null);
                            sourceInfoCollector.Add(lambda.SourceInfo);
                        }
                    } else if (inType == TangentType.PotentiallyAnything) {
                        var pe = inputEnum.Current as ParenExpression;
                        var resolution = pe.TryResolve(scope, element.Parameter.RequiredArgumentType);
                        if (!resolution.Any()) {
                            return PhraseMatchResult.Failure;
                        }

                        Expression resolvedParenExpression = null;

                        if (resolution.Count() > 1) {
                            resolvedParenExpression = new AmbiguousExpression(resolution);
                        } else {
                            resolvedParenExpression = resolution.First();
                        }

                        parameterCollector.Add(resolvedParenExpression);
                        conversionCollector.Add(null);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    } else if (element.Parameter.RequiredArgumentType.ImplementationType == KindOfType.Delegate &&
                         !((DelegateType)element.Parameter.RequiredArgumentType).Takes.Any() &&
                         inType.ImplementationType != KindOfType.Delegate &&
                         ((DelegateType)element.Parameter.RequiredArgumentType).Returns.CompatibilityMatches(inType, inferenceCollector)) {

                        // We have something like ~>int == int
                        parameterCollector.Add(new LambdaExpression(Enumerable.Empty<ParameterDeclaration>(), inType, new Block(new[] { inputEnum.Current }, Enumerable.Empty<ParameterDeclaration>()), inputEnum.Current.SourceInfo));
                        conversionCollector.Add(null);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    } else if (!element.Parameter.RequiredArgumentType.CompatibilityMatches(inType, inferenceCollector)) {
                        var implicitConversion = scope.Conversions.FindConversion(inputEnum.Current.EffectiveType, element.Parameter.RequiredArgumentType);
                        if (implicitConversion == null) {
                            return PhraseMatchResult.Failure;
                        } else {
                            var conversionExpr = implicitConversion.Convert(inputEnum.Current, scope);
                            if (!element.Parameter.RequiredArgumentType.CompatibilityMatches(conversionExpr.EffectiveType, inferenceCollector)) {
                                // Throw? This probably shouldn't happen, but seems to when a generic infers something different than has already been inferred.
                                return PhraseMatchResult.Failure;
                            }

                            parameterCollector.Add(conversionExpr);
                            conversionCollector.Add(implicitConversion);
                            sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                        }
                    } else {
                        parameterCollector.Add(inputEnum.Current);
                        conversionCollector.Add(null);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    }
                }
            }

            foreach (var entry in lazyLambdaMatches) {
                List<ParameterDeclaration> missingInferences = new List<ParameterDeclaration>();
                var targetDelegateType = entry.Item2.ResolveGenericReferences(pd => {
                    if (inferenceCollector.ContainsKey(pd)) {
                        return inferenceCollector[pd];
                    }

                    missingInferences.Add(pd);
                    return TangentType.PotentiallyAnything;
                });

                if (missingInferences.Any()) {
                    return PhraseMatchResult.Failure;
                }

                var lambda = entry.Item1.TryToFitIn(targetDelegateType);
                if (lambda == null) {
                    return PhraseMatchResult.Failure;
                }

                for (int ix = 0; ix < parameterCollector.Count; ++ix) {
                    if (parameterCollector[ix] == entry.Item1) {
                        parameterCollector[ix] = lambda;
                        sourceInfoCollector[ix] = lambda.SourceInfo;
                    }
                }
            }

            return new PhraseMatchResult(Pattern.Count(), LineColumnRange.CombineAll(sourceInfoCollector), parameterCollector, inferenceCollector, conversionCollector);
        }
    }
}
