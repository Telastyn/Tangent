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
            return scopedCache.GetOrAdd(towardsType, t => 
                // Remember, void statements are invariant to the return type. They are already compiled.
                scope.InterpretTowards(towardsType, LastStatement).Select(interpretation =>
                    new FunctionInvocationExpression(
                        new ReductionDeclaration(
                            Enumerable.Empty<PhrasePart>(),
                            new Function(
                                towardsType,
                                new Block(VoidStatements.Statements.Concat(new[] { interpretation }), Enumerable.Empty<ParameterDeclaration>()))),
                        Enumerable.Empty<Expression>(),
                        Enumerable.Empty<TangentType>(),
                        SourceInfo))
            );
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newBlock = VoidStatements.ReplaceParameterAccesses(mapping);
            var newLast = LastStatement.Select(expr => expr.ReplaceParameterAccesses(mapping));

            if(VoidStatements == newBlock && newLast.SequenceEqual(LastStatement)) {
                return this;
            }

            return new ParenExpression(newBlock, newLast.ToList(), SourceInfo);
        }
    }
}
