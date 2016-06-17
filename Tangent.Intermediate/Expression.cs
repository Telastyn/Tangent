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
        //FunctionBinding = 3,
        FunctionInvocation = 4,
        TypeAccess = 5,
        //HalfBoundExpression = 6,
        //DelegateInvocation = 7,
        Constant = 8,
        EnumValueAccess = 9,
        EnumWidening = 10,
        ParenExpr = 11,
        CtorParamAccess = 12,
        GenericParameterAccess = 13,
        TypeInference = 14,
        Ambiguity = 15,
        PartialLambda = 16,
        Lambda = 17,
        DelegateInvocation = 18,
        InvalidProgramException = 19,
        LocalAccess = 20,
        LocalAssignment = 21,
        InitializerPlaceholder = 22,
        CtorCall = 23
    }

    public abstract class Expression
    {
        public abstract ExpressionNodeType NodeType { get; }
        public readonly LineColumnRange SourceInfo;
        public abstract TangentType EffectiveType { get; }

        protected Expression(LineColumnRange sourceInfo)
        {
            SourceInfo = sourceInfo;
        }

        internal virtual void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
        }
    }
}
