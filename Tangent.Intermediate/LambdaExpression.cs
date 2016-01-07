using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LambdaExpression : Expression
    {
        public readonly IEnumerable<ParameterDeclaration> ResolvedParameters;
        public readonly TangentType ResolvedReturnType;
        private readonly DelegateType resolvedType;
        public readonly Block Implementation;

        public LambdaExpression(IEnumerable<ParameterDeclaration> parameters, TangentType returnType, Block implementation, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            resolvedType = DelegateType.For(parameters.Select(pd=>pd.Returns), returnType);
            ResolvedParameters = parameters;
            ResolvedReturnType = returnType;
            Implementation = implementation;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.Lambda; }
        }

        public override TangentType EffectiveType
        {
            get { return resolvedType; }
        }

        public override string ToString()
        {
            return string.Format("{0} {{...}}", resolvedType);
        }
    }
}
