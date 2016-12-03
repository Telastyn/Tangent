using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class LocalAssignment : ExpressionDeclaration
    {
        public readonly ParameterDeclaration Local;

        public LocalAssignment(ParameterDeclaration local) : base(new Phrase(local.Takes.Concat(new[] { new PhrasePart("="), new PhrasePart(new ParameterDeclaration("value", local.Returns)) })))
        {
            Local = local;
        }

        public override Expression Reduce(PhraseMatchResult input)
        {
            //if (input.GenericArguments.Any()) {
            //    throw new ApplicationException("Unexpected input to Local Access.");
            //}

            return new LocalAssignmentExpression(new LocalAccessExpression(Local, input.MatchLocation), input.IncomingArguments.First());
        }

        public override TransformationType Type
        {
            get { return TransformationType.LocalVariable; }
        }
    }
}
