using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate {
    public class CtorCall : Function {
        public CtorCall(BoundGenericProductType type) : base(type, null) { }
        public CtorCall(ProductType type) : base(type, null) { }
        public CtorCall(SumType type) : base(type, null) { }
        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset) {
            // nada.
        }
    }

    public class CtorCallExpression : Expression
    {
        public readonly TangentType Target;
        public readonly IEnumerable<Expression> Arguments;

        public CtorCallExpression(ProductType type) : base(null)
        {
            Target = type.ResolveGenericReferences(generic => GenericArgumentReferenceType.For(generic));
            Arguments = type.DataConstructorParts.Where(pp => !pp.IsIdentifier).Select(pp => new ParameterAccessExpression(pp.Parameter, null));
        }

        public CtorCallExpression(BoundGenericProductType type) : base(null)
        {
            Target = type;
            Arguments = type.GenericProductType.DataConstructorParts.Where(pp => !pp.IsIdentifier).Select(pp => new ParameterAccessExpression(pp.Parameter, null));
        }

        public CtorCallExpression(SumType sum, ParameterDeclaration value) : base(null)
        {
            Target = sum;
            Arguments = new Expression[] { new ParameterAccessExpression(value, null) };
        }

        public Function GenerateWrappedFunction()
        {
            return new Function(Target, new Block(new Expression[] { this }, Enumerable.Empty<ParameterDeclaration>()));
        }

        public override TangentType EffectiveType
        {
            get
            {
                return Target;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.CtorCall;
            }
        }
    }
}
