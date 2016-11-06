using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ConstructorParameterAccess : ExpressionDeclaration
    {
        private readonly ParameterDeclaration ThisParameter;
        private readonly ParameterDeclaration ConstructorParameter;

        public ConstructorParameterAccess(ParameterDeclaration thisParam, ParameterDeclaration ctorParam)
            : base(new Phrase(ctorParam.Takes))
        {
            ThisParameter = thisParam;
            ConstructorParameter = ctorParam;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.IncomingArguments.Any() || input.GenericArguments.Any()) {
                throw new ApplicationException("Unexpected input to Constructor Parameter Access.");
            }

            return new CtorParameterAccessExpression(ThisParameter, ConstructorParameter, input.IncomingArguments, input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.ConstructorReference; }
        }

        public static IEnumerable<ConstructorParameterAccess> For(ParameterDeclaration thisParam, IEnumerable<ParameterDeclaration> ctorParams)
        {
            return ctorParams.Select(pd => new ConstructorParameterAccess(thisParam, pd));
        }
    }
}
