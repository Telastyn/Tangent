using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
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

        public IEnumerable<Expression> TryResolve(Scope scope, TangentType towardsType)
        {
            var input = new Input(LastStatement, scope);

            if (towardsType is LazyType)
            {
                var targetType = ((LazyType)towardsType).Type;
                var result = input.InterpretTowards(targetType);

                if (result.Any())
                {
                    return result.Select(interpretation =>
                        new FunctionBindingExpression(
                        new ReductionDeclaration(
                            Enumerable.Empty<PhrasePart>(),
                            new Function(
                                targetType,
                                new Block(VoidStatements.Statements.Concat(new[] { interpretation })))),
                        Enumerable.Empty<Expression>(), SourceInfo));
                }
            }

            return input.InterpretTowards(towardsType).Select(interpretation =>
                new FunctionInvocationExpression(
                    new FunctionBindingExpression(
                    new ReductionDeclaration(
                        Enumerable.Empty<PhrasePart>(),
                        new Function(
                            towardsType,
                            new Block(VoidStatements.Statements.Concat(new[] { interpretation })))),
                    Enumerable.Empty<Expression>(), SourceInfo)));
        }
    }
}
