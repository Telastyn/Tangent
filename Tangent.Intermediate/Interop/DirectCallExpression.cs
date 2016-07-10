using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectCallExpression : Expression
    {
        public readonly IEnumerable<Expression> Arguments;
        public readonly IEnumerable<TangentType> GenericArguments;
        private readonly TangentType effectiveType;
        public readonly MethodInfo Target;

        public DirectCallExpression(MethodInfo target, TangentType effectiveType, IEnumerable<Expression> args, IEnumerable<TangentType> genericArgs) : base(null)
        {
            Arguments = new List<Expression>(args);
            GenericArguments = new List<TangentType>(genericArgs);
            Target = target;
            this.effectiveType = effectiveType;
        }

        public DirectCallExpression(MethodInfo target, TangentType effectiveType, IEnumerable<ParameterDeclaration> args, IEnumerable<TangentType> genericArgs) : base(null)
        {
            Arguments = new List<Expression>(args.Select(pd => new ParameterAccessExpression(pd, null)));
            GenericArguments = new List<TangentType>(genericArgs);
            this.effectiveType = effectiveType;
            Target = target;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return effectiveType;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.DirectCall;
            }
        }
    }
}
