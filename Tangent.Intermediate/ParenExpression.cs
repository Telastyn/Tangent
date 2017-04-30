using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate
{
    public class ParenExpression : Expression
    {
        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.ParenExpr; }
        }

        public readonly Block VoidStatements;
        public readonly List<Expression> LastStatement;

        public ParenExpression(Block notLastStatements, List<Expression> lastStatement, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            VoidStatements = notLastStatements;
            LastStatement = lastStatement;
        }

        public bool IsSimpleParenExpr
        {
            get
            {
                return !VoidStatements.Statements.Any();
            }
        }

        public override TangentType EffectiveType
        {
            get { return TangentType.PotentiallyAnything; }
        }

        private readonly ConcurrentDictionary<TransformationScope, ConcurrentDictionary<TangentType, IEnumerable<Expression>>> resolutionCache = new ConcurrentDictionary<TransformationScope, ConcurrentDictionary<TangentType, IEnumerable<Expression>>>();

        public IEnumerable<Expression> TryResolve(TransformationScope scope, TangentType towardsType)
        {
            var scopedCache = resolutionCache.GetOrAdd(scope, s => new ConcurrentDictionary<TangentType, IEnumerable<Expression>>());

            // If we're looking to fit into a lazy type, assume the parens are the lazy part.
            if (towardsType.ImplementationType == KindOfType.Delegate && !((DelegateType)towardsType).Takes.Any()) {
                return scopedCache.GetOrAdd(towardsType, t =>
                    // Remember, void statements are invariant to the return type. They are already compiled.
                    scope.InterpretTowards(((DelegateType)towardsType).Returns, LastStatement).Select(interpretation =>
                    new LambdaExpression(
                        Enumerable.Empty<ParameterDeclaration>(),
                        ((DelegateType)towardsType).Returns,
                        new Block(VoidStatements.Statements.Concat(new[] { interpretation }), VoidStatements.Locals),
                        SourceInfo)));
            }

            return scopedCache.GetOrAdd(towardsType, t =>
                // Remember, void statements are invariant to the return type. They are already compiled.
                scope.InterpretTowards(towardsType, LastStatement).Select(interpretation =>
                    new ResolvedParenExpression(new Block(VoidStatements.Statements.Concat(new[] { interpretation }), VoidStatements.Locals), towardsType, SourceInfo)));
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newBlock = VoidStatements.ReplaceParameterAccesses(mapping);
            var newLast = LastStatement.Select(expr => expr.ReplaceParameterAccesses(mapping));

            if (VoidStatements == newBlock && newLast.SequenceEqual(LastStatement)) {
                return this;
            }

            return new ParenExpression(newBlock, newLast.ToList(), SourceInfo);
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
