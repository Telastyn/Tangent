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
        public PhraseMatchResult(int length, Dictionary<ParameterDeclaration, TangentType> genericInferences = null)
        {
            GenericInferences = genericInferences == null ? new Dictionary<ParameterDeclaration, TangentType>() : new Dictionary<ParameterDeclaration, TangentType>(genericInferences);
            Length = length;
        }
    }
}
