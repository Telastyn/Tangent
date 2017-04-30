using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LocalAssignmentExpression : Expression
    {
        public readonly LocalAccessExpression Local;
        public readonly Expression Value;

        public LocalAssignmentExpression(LocalAccessExpression local, Expression value) : base(local.SourceInfo.Combine(value.SourceInfo))
        {
            Local = local;
            Value = value;
        }

        public override TangentType EffectiveType
        {
            get
            {
                return TangentType.Void;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.LocalAssignment;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            Local.ReplaceTypeResolvedFunctions(replacements, workset);
            Value.ReplaceTypeResolvedFunctions(replacements, workset);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Local.RequiresClosureAround(parameters, workset) || Value.RequiresClosureAround(parameters, workset);
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Local.AccessesAnyParameters(parameters, workset) || Value.AccessesAnyParameters(parameters, workset);
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach (var arg in Value.CollectLocals(workset)) {
                yield return arg;
            }
        }
    }
}
