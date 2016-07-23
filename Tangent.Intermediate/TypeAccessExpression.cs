﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class TypeAccessExpression : Expression
    {
        public readonly TypeConstant TypeConstant;

        public override ExpressionNodeType NodeType
        {
            get { return ExpressionNodeType.TypeAccess; }
        }

        public override TangentType EffectiveType
        {
            get { return TypeConstant; }
        }

        public TypeAccessExpression(TypeConstant type, LineColumnRange sourceInfo)
            : base(sourceInfo)
        {
            TypeConstant = type;
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }
    }
}
