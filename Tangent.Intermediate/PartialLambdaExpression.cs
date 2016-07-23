using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PartialLambdaExpression : Expression
    {
        public readonly List<ParameterDeclaration> GenericParameters;
        public readonly List<ParameterDeclaration> Parameters;
        public readonly TransformationScope ContainingScope;
        private readonly Func<TransformationScope, TangentType, Expression> resolver;
        private readonly ConcurrentDictionary<TangentType, Expression> cache = new ConcurrentDictionary<TangentType, Expression>();
        private readonly DelegateType fullInferenceType;

        public PartialLambdaExpression(IEnumerable<ParameterDeclaration> parameters, TransformationScope containingScope, Func<TransformationScope, TangentType, Expression> resolver, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            var generics = new List<ParameterDeclaration>() { new ParameterDeclaration("<lambda-return>", TangentType.Any.Kind) };
            foreach (var entry in parameters) {
                if (entry.Returns == null) {
                    generics.Add(new ParameterDeclaration(string.Format("<lambda-generic{0}>", generics.Count), TangentType.Any.Kind));
                    entry.Returns = GenericInferencePlaceholder.For(generics.Last());
                }
            }

            this.resolver = resolver;
            this.GenericParameters = generics;
            this.Parameters = new List<ParameterDeclaration>(parameters);
            this.fullInferenceType = DelegateType.For(parameters.Select(p => p.Returns), GenericInferencePlaceholder.For(generics.First()));
            this.ContainingScope = containingScope;
        }

        public Expression TryToFitIn(TangentType target)
        {
            return cache.GetOrAdd(target, tt => ReallyTryToFitIn(tt));
        }

        private Expression ReallyTryToFitIn(TangentType target){
            var inferenceCollector = new Dictionary<ParameterDeclaration, TangentType>();
            if (!fullInferenceType.CompatibilityMatches(target, inferenceCollector)) {
                return null;
            }

            // Inference works? Great. See if the block actually works.
            var realParams = new List<ParameterDeclaration>();
            foreach (var entry in Parameters) {
                realParams.Add(new ParameterDeclaration(entry.Takes, entry.Returns.ImplementationType == KindOfType.InferencePoint ? inferenceCollector[((GenericInferencePlaceholder)entry.Returns).GenericArgument] : entry.Returns));
            }

            var returnType = inferenceCollector[((GenericInferencePlaceholder)fullInferenceType.Returns).GenericArgument];
            var newScope = new TransformationScope(ContainingScope.Rules.SelectMany(x => x).Concat(realParams.Select(p => new ParameterAccess(p))));
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
    }
}
