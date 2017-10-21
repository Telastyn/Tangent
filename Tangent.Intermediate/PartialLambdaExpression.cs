using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PartialLambdaExpression : Expression, FitableLambda
    {
        public readonly List<ParameterDeclaration> GenericParameters;
        public readonly List<ParameterDeclaration> Parameters;
        public readonly TransformationScope ContainingScope;
        private readonly Func<TransformationScope, TangentType, Expression> resolver;
        private readonly ConcurrentDictionary<bool, ConcurrentDictionary<TangentType, Expression>> cache = new ConcurrentDictionary<bool, ConcurrentDictionary<TangentType, Expression>>();
        private readonly DelegateType fullInferenceType;

        public PartialLambdaExpression(IEnumerable<ParameterDeclaration> parameters, TransformationScope containingScope, Func<TransformationScope, TangentType, Expression> resolver, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            var generics = new List<ParameterDeclaration>() { new ParameterDeclaration("<lambda-return>", TangentType.Any.Kind) };
            foreach (var entry in parameters) {
                if (entry.Returns == null) {
                    generics.Add(new ParameterDeclaration(string.Format("<lambda-generic{0}>", generics.Count), TangentType.Any.Kind));
                    entry.Returns = GenericArgumentReferenceType.For(generics.Last());
                }
            }

            this.resolver = resolver;
            this.GenericParameters = generics;
            this.Parameters = new List<ParameterDeclaration>(parameters);
            this.fullInferenceType = DelegateType.For(parameters.Select(p => p.Returns), GenericArgumentReferenceType.For(generics.First()));
            this.ContainingScope = containingScope;
        }

        public Expression TryToFitIn(TangentType target)
        {
            return TryToFitIn(target, false);
        }

        public Expression TryToFitIn(TangentType target, bool useGroupSemantics)
        {
            return cache.GetOrAdd(useGroupSemantics, new ConcurrentDictionary<TangentType, Expression>()).GetOrAdd(target, tt => ReallyTryToFitIn(tt, useGroupSemantics));
        }

        private Expression ReallyTryToFitIn(TangentType target, bool useGroupSemantics)
        {
            var inferenceCollector = new Dictionary<ParameterDeclaration, TangentType>();
            if (!useGroupSemantics) {

                // We inference match the whole thing.
                if (!fullInferenceType.CompatibilityMatches(target, inferenceCollector)) {
                    return null;
                }
            } else {
                // We need to exclude partial generic parameters, since the (T) there is inferred from the real type, not the parameter the lambda is being passed to.
                // Also, we don't care if the parameter part fits.
                if (fullInferenceType.Takes.All(t => t.ImplementationType == KindOfType.GenericReference)) {

                    // We inference match the whole thing.
                    if (!fullInferenceType.CompatibilityMatches(target, inferenceCollector)) {
                        return null;
                    }
                } else {

                    // We only inference match the return.
                    if (target.ImplementationType != KindOfType.Delegate || !fullInferenceType.Returns.CompatibilityMatches((target as DelegateType).Returns, inferenceCollector)) {
                        return null;
                    }
                }

            }

            // Inference works? Great. See if the block actually works.
            var realParams = new List<ParameterDeclaration>();
            foreach (var entry in Parameters) {
                realParams.Add(new ParameterDeclaration(entry.Takes, entry.Returns.ImplementationType == KindOfType.GenericReference ? inferenceCollector[((GenericArgumentReferenceType)entry.Returns).GenericParameter] : entry.Returns));
            }

            var returnType = inferenceCollector[((GenericArgumentReferenceType)fullInferenceType.Returns).GenericParameter];
            var newScope = ContainingScope.CreateNestedParameterScope(realParams);
            var implementation = resolver(newScope, returnType);
            if (implementation == null) {
                return null;
            }

            if (implementation.NodeType == ExpressionNodeType.Ambiguity) {
                return implementation;
            }

            if (implementation.NodeType == ExpressionNodeType.ParenExpr) {
                return new LambdaExpression(realParams, returnType, ((ParenExpression)implementation).VoidStatements, SourceInfo);
            }

            throw new ApplicationException("Unexpected expression type during lambda compatibility compilation.");
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.PartialLambda; }
        }

        public override TangentType EffectiveType
        {
            get { return TangentType.PotentiallyAnything; }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this; // works? no idea.
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            yield break;
        }
    }
}
