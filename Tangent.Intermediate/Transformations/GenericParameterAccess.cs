using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class GenericParameterAccess : ExpressionDeclaration
    {
        private readonly ParameterDeclaration GenericDeclaration;

        public GenericParameterAccess(ParameterDeclaration generic)
            : base(new Phrase(generic.Takes))
        {
            if (generic.Takes.Any(t => !t.IsIdentifier)) {
                throw new InvalidOperationException("Generics may not take parameters.");
            }

            GenericDeclaration = generic;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.IncomingArguments.Any() || input.GenericArguments.Any()) {
                throw new ApplicationException("Unexpected input to Generic Parameter Access.");
            }

            return new GenericParameterAccessExpression(GenericDeclaration, input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.GenericParam; }
        }
    }
}
