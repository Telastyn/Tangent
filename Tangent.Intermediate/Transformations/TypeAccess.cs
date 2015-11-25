﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Transformations
{
    public class TypeAccess : ExpressionDeclaration
    {
        private readonly TypeDeclaration Declaration;
        public TypeAccess(TypeDeclaration decl)
            : base(new Phrase(decl.Takes))
        {
            Declaration = decl;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (Declaration.IsGeneric) {
                var genericBindingArgs = input.IncomingParameters.Select(expr => expr.EffectiveType.ImplementationType == KindOfType.TypeConstant ? ((TypeConstant)expr.EffectiveType).Value : expr.EffectiveType).ToList();
                var genericBinding = BoundGenericType.For(Declaration, genericBindingArgs);
                return new TypeAccessExpression(genericBinding.TypeConstant, input.MatchLocation);
            } else {
                if (input.IncomingParameters.Any() || input.GenericInferences.Any()) {
                    throw new ApplicationException("Unexpected input to Type Access.");
                }

                return new TypeAccessExpression(Declaration.Returns.TypeConstant, input.MatchLocation);
            }
        }

        public override TransformationType Type
        {
            get { return TransformationType.Type; }
        }
    }
}
