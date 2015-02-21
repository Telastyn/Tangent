using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public enum ExpressionNodeType
    {
        Unknown = 0,
        Identifier = 1,
        ParameterAccess = 2,
        FunctionBinding = 3,
        FunctionInvocation = 4,
        TypeAccess = 5,
        //HalfBoundExpression = 6,
        DelegateInvocation = 7,
        Constant = 8,
        EnumValueAccess = 9,
        EnumWidening = 10,
        ParenExpr = 11
    }

    public abstract class Expression
    {
        public abstract ExpressionNodeType NodeType { get; }
        internal virtual void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
        }
    }
}
