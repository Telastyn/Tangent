using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class LambdaGroupElement : PartialElement
    {
        public readonly IEnumerable<PartialElement> InputExpr;
        public readonly IEnumerable<LambdaElement> Lambdas;

        public LambdaGroupElement(IEnumerable<PartialElement> inputExpr, IEnumerable<LambdaElement> lambdas, LineColumnRange sourceInfo):base(ElementType.LambdaGroup, sourceInfo)
        {
            InputExpr = inputExpr;
            Lambdas = lambdas;
        }
    }
}
