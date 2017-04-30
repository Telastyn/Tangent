using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectAssignElementExpression : Expression
    {
        public readonly Expression ArrayAccess;
        public readonly Expression IndexAccess;
        public readonly Expression Assignment;
        public readonly TangentType ArrayType;

        public DirectAssignElementExpression(Expression arrayAccess, Expression indexAccess, Expression assignment, TangentType arrayType) : base(null)
        {
            this.ArrayAccess = arrayAccess;
            this.IndexAccess = indexAccess;
            this.Assignment = assignment;
            this.ArrayType = arrayType;
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
                return ExpressionNodeType.DirectElementAssignment;
            }
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return ArrayAccess.AccessesAnyParameters(parameters, workset) || IndexAccess.AccessesAnyParameters(parameters, workset) || Assignment.AccessesAnyParameters(parameters, workset);
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var arrayAccess = ArrayAccess.ReplaceParameterAccesses(mapping);
            var indexAccess = IndexAccess.ReplaceParameterAccesses(mapping);
            var assignment = Assignment.ReplaceParameterAccesses(mapping);

            if (arrayAccess != ArrayAccess || indexAccess != IndexAccess || assignment != Assignment) {
                return new DirectAssignElementExpression(arrayAccess, indexAccess, assignment, ArrayType);
            }

            return this;
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return ArrayAccess.RequiresClosureAround(parameters, workset) || IndexAccess.RequiresClosureAround(parameters, workset) || Assignment.RequiresClosureAround(parameters, workset);
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            if (workset.Contains(this)) { yield break; }
            workset.Add(this);

            foreach (var arg in ArrayAccess.CollectLocals(workset)) {
                yield return arg;
            }

            foreach (var arg in IndexAccess.CollectLocals(workset)) {
                yield return arg;
            }

            foreach (var arg in Assignment.CollectLocals(workset)) {
                yield return arg;
            }
        }

        public override string ToString()
        {
            return $"{ArrayAccess}[{IndexAccess}] = {Assignment}";
        }
    }
}
