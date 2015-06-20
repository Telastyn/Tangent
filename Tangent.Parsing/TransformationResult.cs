using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public class TransformationResult
    {
        public readonly int Takes;
        public readonly Expression ReplacesWith;

        public TransformationResult(int count, Expression replacesWith)
        {
            Takes = count;
            ReplacesWith = replacesWith;
        }
    }
}
