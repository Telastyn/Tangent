using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ParameterAccess : ExpressionDeclaration
    {
        private readonly ParameterDeclaration Parameter;
        public ParameterAccess(ParameterDeclaration parameter)
            : base(new Phrase(parameter.Takes.Select(id => new PhrasePart(id))))
        {
            Parameter = parameter;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.IncomingArguments.Any() || input.GenericInferences.Any()) {
                throw new ApplicationException("Unexpected input to Parameter Access.");
            }

            return new ParameterAccessExpression(Parameter, input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.FunctionParameter; }
        }
    }
}
