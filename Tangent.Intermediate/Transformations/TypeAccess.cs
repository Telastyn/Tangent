﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TypeAccess : ExpressionDeclaration
    {
        public readonly TypeDeclaration Declaration;
        public TypeAccess(TypeDeclaration decl)
            : base(new Phrase(decl.Takes))
        {
            Declaration = decl;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (Declaration.IsGeneric) {
                var generic = Declaration.Returns as HasGenericParameters;
                if (generic != null) {
                    var genericBinding = BoundGenericType.For(generic, Declaration.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).Select(gp => input.GenericArguments[gp]).ToList());
                    return new TypeAccessExpression(genericBinding.TypeConstant, input.MatchLocation);
                } else {
                    var genericBinding = Declaration.Returns.ResolveGenericReferences(pd => input.GenericArguments[pd]);
                    return new TypeAccessExpression(genericBinding.TypeConstant, input.MatchLocation);
                }
            } else {
                if (input.IncomingArguments.Any() || input.GenericArguments.Any()) {
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
