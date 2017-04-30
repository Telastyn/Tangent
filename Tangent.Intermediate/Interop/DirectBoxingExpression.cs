using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectBoxingExpression : Expression
    {
        public readonly Expression Target;
        public DirectBoxingExpression(Expression target) : base(null)
        {
            Target = target;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return DotNetType.For(typeof(object));
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.DirectBox;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var newb = Target.ReplaceParameterAccesses(mapping);
            if (newb == Target) {
                return this;
            }

            return new DirectBoxingExpression(newb);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Target.RequiresClosureAround(parameters, workset);
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Target.AccessesAnyParameters(parameters, workset);
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach (var arg in Target.CollectLocals(workset)) {
                yield return arg;
            }
        }
    }
}
