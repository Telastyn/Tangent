using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class DirectAccessElementExpression : Expression
    {
        public readonly Expression ArrayAccess;
        public readonly Expression IndexAccess;
        private readonly TangentType effectiveType;

        public DirectAccessElementExpression(Expression arrayAccess, Expression indexAccess, TangentType effectiveType) : base(null)
        {
            this.ArrayAccess = arrayAccess;
            this.IndexAccess = indexAccess;
            this.effectiveType = effectiveType;
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
                return ExpressionNodeType.DirectElementAccess;
            }
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return ArrayAccess.AccessesAnyParameters(parameters, workset) || IndexAccess.AccessesAnyParameters(parameters, workset);
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            var arrayAccess = ArrayAccess.ReplaceParameterAccesses(mapping);
            var indexAccess = IndexAccess.ReplaceParameterAccesses(mapping);

            if(arrayAccess != ArrayAccess || indexAccess != IndexAccess) {
                return new DirectAccessElementExpression(arrayAccess, indexAccess, effectiveType);
            }

            return this;
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            if (workset.Contains(this)) {
                return false;
            }

            workset.Add(this);

            return ArrayAccess.RequiresClosureAround(parameters, workset) || IndexAccess.RequiresClosureAround(parameters, workset);
        }

        public override string ToString()
        {
            return $"{ArrayAccess}[{IndexAccess}]";
        }
    }
}
