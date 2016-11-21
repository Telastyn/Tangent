using System;
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
                if (Declaration.Returns.ImplementationType == KindOfType.Product) {
                    var generic = (ProductType)Declaration.Returns;
                    var genericBinding = BoundGenericProductType.For(generic, generic.GenericParameters.Select(gp => input.GenericArguments[gp]));
                    return new TypeAccessExpression(genericBinding.TypeConstant, input.MatchLocation);
                } else {
                    var genericBinding = BoundGenericType.For(Declaration, Declaration.Takes.Where(pp=>!pp.IsIdentifier).Select(pp=>input.GenericArguments[pp.Parameter]));
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
