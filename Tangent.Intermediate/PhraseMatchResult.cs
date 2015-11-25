using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PhraseMatchResult
    {
        public readonly int Length;
        public readonly IEnumerable<Expression> IncomingParameters;
        public readonly Dictionary<ParameterDeclaration, TangentType> GenericInferences;
        public bool Success
        {
            get
            {
                return GenericInferences != null;
            }
        }

        public static readonly PhraseMatchResult Failure = new PhraseMatchResult();

        private PhraseMatchResult() { }
        public PhraseMatchResult(int length, IEnumerable<Expression> matchedParameters = null, Dictionary<ParameterDeclaration, TangentType> genericInferences = null)
        {
            IncomingParameters = matchedParameters ?? Enumerable.Empty<Expression>();
            GenericInferences = genericInferences == null ? new Dictionary<ParameterDeclaration, TangentType>() : new Dictionary<ParameterDeclaration, TangentType>(genericInferences);
            Length = length;
        }
    }
}
