using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Intermediate
{
    public class TransformationResult
    {
        public readonly int Takes;
        public readonly List<ConversionPath> ConversionInfo;
        public readonly Expression ReplacesWith;
        public bool Success
        {
            get
            {
                return ReplacesWith != null;
            }
        }

        public TransformationResult(int count, IEnumerable<ConversionPath> conversionInfo, Expression replacesWith)
        {
            Takes = count;
            ReplacesWith = replacesWith;
            ConversionInfo = new List<ConversionPath>(conversionInfo);
        }

        public static readonly TransformationResult Failure = new TransformationResult(0, Enumerable.Empty<ConversionPath>(), null);
    }
}
