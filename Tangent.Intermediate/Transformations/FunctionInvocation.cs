using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Transformations
{
    public class FunctionInvocation:ExpressionDeclaration
    {
        public readonly ReductionDeclaration Declaration;

        public FunctionInvocation(ReductionDeclaration declaration)
            : base(new Phrase(declaration.Takes))
        {
            Declaration = declaration;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            return new FunctionInvocationExpression(Declaration, input.IncomingArguments, Declaration.GenericParameters.Select(gp => input.GenericInferences[gp]), input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.Function; }
        }
    }
}
