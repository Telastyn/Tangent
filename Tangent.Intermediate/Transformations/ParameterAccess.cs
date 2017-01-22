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
            : base(new Phrase(parameter.Takes.Select(pp => pp.ResolveGenericReferences(pd => UninferredGenericType.For(pd)))))
        {
            Parameter = parameter;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.GenericArguments.Any()) {
                throw new ApplicationException("Unexpected input to Parameter Access.");
            }

            var paramAccess = new ParameterAccessExpression(Parameter, input.MatchLocation);
            if (input.IncomingArguments.Any()) {
                return new DelegateInvocationExpression(paramAccess, input.IncomingArguments, input.MatchLocation);
            } else {
                return paramAccess;
            }
        }

        public override TransformationType Type
        {
            get { return TransformationType.FunctionParameter; }
        }
    }
}
