using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class CtorParameterAccessExpression : Expression
    {
        public readonly ParameterDeclaration ThisParam;
        public readonly ParameterDeclaration CtorParam;
        public readonly ParameterDeclaration EffectiveParam;
        public readonly IEnumerable<Expression> Arguments;

        public CtorParameterAccessExpression(ParameterDeclaration thisParam, ParameterDeclaration ctorParam, ParameterDeclaration effectiveParam, IEnumerable<Expression> arguments, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            this.ThisParam = thisParam;
            this.CtorParam = ctorParam;
            this.EffectiveParam = effectiveParam;
            this.Arguments = arguments;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.CtorParamAccess; }
        }

        public override TangentType EffectiveType
        {
            get
            {
                return EffectiveParam.RequiredArgumentType;
            }
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // noop.
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            if (mapping.ContainsKey(ThisParam) || mapping.ContainsKey(CtorParam)) {
                throw new NotImplementedException("ReplaceParameterAccesses called with this/ctor param. This is unsupported.");
            }

            var newbs = Arguments.Select(expr => expr.ReplaceParameterAccesses(mapping));
            if (Arguments.SequenceEqual(newbs)) {
                return this;
            }

            return new CtorParameterAccessExpression(ThisParam, CtorParam, EffectiveParam, newbs, SourceInfo);
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

            foreach (var arg in Arguments.SelectMany(arg => arg.CollectLocals(workset))) {
                yield return arg;
            }
        }
    }
}
