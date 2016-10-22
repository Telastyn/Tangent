using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FunctionInvocation : ExpressionDeclaration
    {
        public readonly ReductionDeclaration Declaration;

        public FunctionInvocation(ReductionDeclaration declaration)
            : base(new Phrase(declaration.Takes))
        {
            Declaration = declaration;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            // TODO: clean this up.
            var parameterBindings = Declaration.Takes.Where(pp => !pp.IsIdentifier && pp.Parameter.Returns.ImplementationType == KindOfType.Kind).Select(pp => pp.Parameter).Zip(input.IncomingArguments, (param, expr) => new { Parameter = param, Expression = expr }).ToDictionary(pair => pair.Parameter, pair => pair.Expression);
            return new FunctionInvocationExpression(Declaration, input.IncomingArguments.Where(expr => !parameterBindings.Values.Contains(expr)), Declaration.GenericParameters.Select(gp => parameterBindings.ContainsKey(gp) ? parameterBindings[gp].EffectiveType : input.GenericInferences[gp]).ToList(), input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.Function; }
        }
    }
}
