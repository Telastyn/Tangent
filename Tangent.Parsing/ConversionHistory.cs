using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public struct ConversionHistory
    {
        public readonly int BufferLength;
        public readonly int ConversionIndex;
        public readonly ReductionDeclaration Conversion;

        public ConversionHistory(ReductionDeclaration conversion, int bufferLength, int conversionIndex)
        {
            this.Conversion = conversion;
            this.BufferLength = bufferLength;
            this.ConversionIndex = conversionIndex;
        }
    }
}
