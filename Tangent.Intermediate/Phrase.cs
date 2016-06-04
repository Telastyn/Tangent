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
            var sourceInfoCollector = new List<LineColumnRange>();
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
                    if (element.Parameter.RequiredArgumentType == TangentType.Any.Kind && (inType.ImplementationType == KindOfType.Kind || inType.ImplementationType == KindOfType.TypeConstant || inType.ImplementationType == KindOfType.GenericReference || inType.ImplementationType == KindOfType.InferencePoint)) {
                        parameterCollector.Add(inputEnum.Current);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    } else if (inputEnum.Current.NodeType == ExpressionNodeType.PartialLambda) {
                        var lambda = ((PartialLambdaExpression)inputEnum.Current).TryToFitIn(element.Parameter.RequiredArgumentType);
                        if (lambda == null) {
                            return PhraseMatchResult.Failure;
                        }

                        parameterCollector.Add(lambda);
                        sourceInfoCollector.Add(lambda.SourceInfo);
                    } else if (inType == TangentType.PotentiallyAnything) {
                        throw new NotImplementedException("Add support for paren expressions.");
                        parameterCollector.Add(inputEnum.Current);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    } else if (element.Parameter.RequiredArgumentType.ImplementationType == KindOfType.Delegate &&
                         !((DelegateType)element.Parameter.RequiredArgumentType).Takes.Any() &&
                         ((DelegateType)element.Parameter.RequiredArgumentType).Returns == inType) {

                        // We have something like ~>int == int
                        parameterCollector.Add(new LambdaExpression(Enumerable.Empty<ParameterDeclaration>(), inType, new Block(new[] { inputEnum.Current }, Enumerable.Empty<ParameterDeclaration>()), inputEnum.Current.SourceInfo));
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    } else if (!element.Parameter.RequiredArgumentType.CompatibilityMatches(inType, inferenceCollector)) {
                        return PhraseMatchResult.Failure;
                    } else {
                        parameterCollector.Add(inputEnum.Current);
                        sourceInfoCollector.Add(inputEnum.Current.SourceInfo);
                    }
                }
            }

            return new PhraseMatchResult(Pattern.Count(), LineColumnRange.CombineAll(sourceInfoCollector), parameterCollector, inferenceCollector);
        }
    }
}
