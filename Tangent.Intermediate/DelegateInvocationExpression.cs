﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class DelegateInvocationExpression : Expression
    {
        public readonly IEnumerable<Expression> Arguments;
        public readonly Expression DelegateAccess;

        public DelegateInvocationExpression(Expression delegateAccess, IEnumerable<Expression> arguments, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            DelegateAccess = delegateAccess;
            Arguments = arguments;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.DelegateInvocation; }
        }

        public DelegateType DelegateType
        {
            get
            {
                var paramAccess = DelegateAccess as ParameterAccessExpression;

                if (paramAccess != null) {
                    return paramAccess.Parameter.RequiredArgumentType as DelegateType;
                } else {
                    return DelegateAccess.EffectiveType as DelegateType;
                }
            }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return DelegateType.Returns;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newbs = Arguments.Select(expr => expr.ReplaceParameterAccesses(mapping));
            var newAccess = DelegateAccess.ReplaceParameterAccesses(mapping);

            if (Arguments.SequenceEqual(newbs) && DelegateAccess == newAccess) {
                return this;
            }

            return new DelegateInvocationExpression(newAccess, newbs, SourceInfo);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return DelegateAccess.RequiresClosureAround(parameters, workset) || Arguments.Any(arg => arg.RequiresClosureAround(parameters, workset));
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return DelegateAccess.AccessesAnyParameters(parameters, workset) || Arguments.Any(arg => arg.AccessesAnyParameters(parameters, workset));
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            foreach(var arg in Arguments) {
                arg.ReplaceTypeResolvedFunctions(replacements, workset);
            }

            DelegateAccess.ReplaceTypeResolvedFunctions(replacements, workset);
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach (var arg in Arguments.SelectMany(arg => arg.CollectLocals(workset))) {
                yield return arg;
            }

            foreach(var arg in DelegateAccess.CollectLocals(workset)) {
                yield return arg;
            }
        }
    }
}
