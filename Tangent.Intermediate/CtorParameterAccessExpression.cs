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

        public CtorParameterAccessExpression(ParameterDeclaration thisParam, ParameterDeclaration ctorParam, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            this.ThisParam = thisParam;
            this.CtorParam = ctorParam;
        }

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.CtorParamAccess; }
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            // noop.
        }
    }
}
