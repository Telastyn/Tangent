using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PhraseMatchResult
    {
        public readonly int TokenMatchLength;
        public readonly IEnumerable<Expression> IncomingArguments;
        public readonly Dictionary<ParameterDeclaration, TangentType> GenericInferences;
        public readonly LineColumnRange MatchLocation;
        public readonly IEnumerable<ConversionPath> ConversionInfo;

        public bool Success
        {
            get
            {
                return GenericInferences != null && !IsAmbiguous;
            }
        }

        public bool IsAmbiguous
        {
            get
            {
                if (IncomingArguments == null) { return false; }
                return IncomingArguments.Any(expr => expr.NodeType == ExpressionNodeType.Ambiguity);
            }
        }

        public static readonly PhraseMatchResult Failure = new PhraseMatchResult();

        private PhraseMatchResult() { }
        public PhraseMatchResult(int tokensMatched, LineColumnRange matchLocation, IEnumerable<Expression> matchedParameters = null, Dictionary<ParameterDeclaration, TangentType> genericInferences = null, IEnumerable<ConversionPath> conversionInfo = null)
        {
            IncomingArguments = matchedParameters ?? Enumerable.Empty<Expression>();
            GenericInferences = genericInferences == null ? new Dictionary<ParameterDeclaration, TangentType>() : new Dictionary<ParameterDeclaration, TangentType>(genericInferences);
            ConversionInfo = conversionInfo ?? Enumerable.Empty<ConversionPath>();
            TokenMatchLength = tokensMatched;
            MatchLocation = matchLocation;
        }
    }
}
