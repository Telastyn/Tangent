using System;
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

        public IEnumerable<Expression> TryResolve(TransformationScope scope, TangentType towardsType)
        {
            // Does this work? How do the void statements get interpreted?
            return scope.InterpretTowards(towardsType, LastStatement).Select(interpretation =>
                new FunctionInvocationExpression(
                    new ReductionDeclaration(
                        Enumerable.Empty<PhrasePart>(),
                        new Function(
                            towardsType,
                            new Block(VoidStatements.Statements.Concat(new[] { interpretation }), Enumerable.Empty<ParameterDeclaration>()))),
                    Enumerable.Empty<Expression>(),
                    Enumerable.Empty<TangentType>(),
                    SourceInfo));
        }
    }
}
