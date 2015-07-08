using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public class GenericSumTypeFunctionWithReturnTypeRelyingOnInference:ParseError
    {
        public readonly ParameterDeclaration TroublesomeGenericReference;
        public readonly IEnumerable<PhrasePart> SumTypeVariant;

        public GenericSumTypeFunctionWithReturnTypeRelyingOnInference(IEnumerable<PhrasePart> sumTypeFunction, ParameterDeclaration badGeneric)
        {
            this.TroublesomeGenericReference = badGeneric;
            this.SumTypeVariant = new List<PhrasePart>(sumTypeFunction);
        }
    }
}
