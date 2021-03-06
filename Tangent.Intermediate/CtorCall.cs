﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class CtorCall : Function
    {
        public CtorCall(BoundGenericType type) : base(type, null) { }
        public CtorCall(ProductType type) : base(type, null) { }
        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // nada.
        }
    }

    public class CtorCallExpression : Expression
    {
        public readonly TangentType Target;
        public readonly IEnumerable<Expression> Arguments;

        public CtorCallExpression(ProductType type, Func<ParameterDeclaration, ParameterDeclaration> paramMapping) : base(null)
        {
            Target = type.ResolveGenericReferences(generic => GenericArgumentReferenceType.For(generic));
            Arguments = type.DataConstructorParts.Where(pp => !pp.IsIdentifier).Select(pp => new ParameterAccessExpression(paramMapping(pp.Parameter), null));
        }

        public CtorCallExpression(BoundGenericType type, Func<ParameterDeclaration, ParameterDeclaration> paramMapping) : base(null)
        {
            Target = type;
            switch (type.GenericType.ImplementationType) {
                case KindOfType.Product:
                    Arguments = ((ProductType)type.GenericType).DataConstructorParts.Where(pp => !pp.IsIdentifier && pp.Parameter.RequiredArgumentType.ImplementationType != KindOfType.Kind).Select(pp => new ParameterAccessExpression(paramMapping(pp.Parameter), null)).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private CtorCallExpression(TangentType target, IEnumerable<Expression> args) : base(null)
        {
            Target = target;
            Arguments = args;
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

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = Arguments.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (Arguments.SequenceEqual(newbs)) {
                return this;
            }

            return new CtorCallExpression(Target, newbs);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return Arguments.Any(arg => arg.RequiresClosureAround(parameters, workset));
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return Arguments.Any(arg => arg.AccessesAnyParameters(parameters, workset));
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach(var arg in Arguments.SelectMany(arg => arg.CollectLocals(workset))) {
                yield return arg;
            }
        }
    }
}
