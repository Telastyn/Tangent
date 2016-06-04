using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LocalAccess : ExpressionDeclaration
    {
        public readonly ParameterDeclaration Local;
        public LocalAccess(ParameterDeclaration local) : base(new Phrase(local.Takes))
        {
            Local = local;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            if (input.GenericInferences.Any()) {
                throw new ApplicationException("Unexpected input to Local Access.");
            }

            return new LocalAccessExpression(Local, input.MatchLocation);
        }

        public override TransformationType Type
        {
            get { return TransformationType.LocalVariable; }
        }

        public static IEnumerable<TransformationRule> RulesForLocal(ParameterDeclaration localVar)
        {
            yield return new LocalAccess(localVar);
            yield return new LocalAssignment(localVar);
        }
    }
}
