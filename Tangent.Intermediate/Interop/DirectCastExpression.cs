using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectCastExpression : Expression
    {
        public readonly Expression Argument;
        public readonly TangentType TargetType;

        public DirectCastExpression(Expression arg, TangentType target) : base(null)
        {
            Argument = arg;
            TargetType = target;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return TargetType;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.DirectCast;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newb = Argument.ReplaceParameterAccesses(mapping);
            if (newb == Argument) { return this; }

            return new DirectCastExpression(newb, TargetType);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Argument.RequiresClosureAround(parameters, workset);
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Argument.AccessesAnyParameters(parameters, workset);
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach (var arg in Argument.CollectLocals(workset)) {
                yield return arg;
            }
        }
    }
}
