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
        public readonly Expression ReplacesWith;
        public bool Success
        {
            get
            {
                return ReplacesWith != null;
            }
        }

        public TransformationResult(int count, Expression replacesWith)
        {
            Takes = count;
            ReplacesWith = replacesWith;
        }

        public static readonly TransformationResult Failure = new TransformationResult(0, null);
    }
}
