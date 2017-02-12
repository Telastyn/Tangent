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
        private readonly ParameterDeclaration EffectiveParameter;

        private ConstructorParameterAccess(ParameterDeclaration thisParam, ParameterDeclaration ctorParam, ParameterDeclaration effectiveParameter = null)
            : base(new Phrase(ctorParam.Takes))
        {
            ThisParameter = thisParam;
            ConstructorParameter = ctorParam;
            EffectiveParameter = effectiveParameter ?? ctorParam;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.IncomingArguments.Any() || input.GenericArguments.Any()) {
                throw new ApplicationException("Unexpected input to Constructor Parameter Access.");
            }

            return new CtorParameterAccessExpression(ThisParameter, ConstructorParameter, EffectiveParameter, input.IncomingArguments, input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.ConstructorReference; }
        }

        public static IEnumerable<ConstructorParameterAccess> For(ParameterDeclaration thisParam, IEnumerable<ParameterDeclaration> ctorParams)
        {
            var boundGeneric = thisParam.Returns as BoundGenericType;
            if (boundGeneric != null) {
                var genericType = boundGeneric.GenericType as HasGenericParameters;
                if (genericType == null) { throw new NotImplementedException(); }
                var mapping = genericType.GenericParameters.Zip(boundGeneric.TypeArguments, (g, a) => new { Generic = g, Argument = a }).ToDictionary(ga => ga.Generic, ga => ga.Argument);
                return ctorParams.Select(pd => new ConstructorParameterAccess(thisParam, pd, pd.ResolveGenericReferences(ga => mapping[ga])));
            } else {
                return ctorParams.Select(pd => new ConstructorParameterAccess(thisParam, pd));
            }
        }
    }
}
