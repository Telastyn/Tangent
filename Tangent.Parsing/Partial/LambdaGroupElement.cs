using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class LambdaGroupElement : PartialElement
    {
        public readonly IEnumerable<LambdaElement> Lambdas;

        public LambdaGroupElement(IEnumerable<LambdaElement> lambdas, LineColumnRange sourceInfo) : base(ElementType.LambdaGroup, sourceInfo)
        {
            if (lambdas == null || !lambdas.Any()) {
                throw new InvalidOperationException("Lambda groups must have at least one lambda.");
            }

            Lambdas = lambdas;
        }
    }
}
