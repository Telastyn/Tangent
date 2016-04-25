using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public abstract class ExpressionDeclaration : TransformationRule
    {
        public readonly Phrase DeclaredPhrase;

        public ExpressionDeclaration(Phrase phrase)
        {
            DeclaredPhrase = phrase;
            MaxTakeCount = phrase.Pattern.Count();
        }

        public TransformationResult TryReduce(List<Expression> input, TransformationScope scope)
        {
            var match = DeclaredPhrase.TryMatch(input, scope);
            if (match.IsAmbiguous) {
                throw new NotImplementedException("fan out ambiguities to return an ambiguous transformation result.");
            }

            if (!match.Success) {
                return TransformationResult.Failure;
            }

            return new TransformationResult(match.TokenMatchLength, Reduce(match));
        }

        public abstract Expression Reduce(PhraseMatchResult input);

        public abstract TransformationType Type
        {
            get;
        }

        public int MaxTakeCount
        {
            get;
            private set;
        }

        public bool IsConversion
        {
            get;
            private set;
        }
    }
}
