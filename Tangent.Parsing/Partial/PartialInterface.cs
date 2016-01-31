using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialInterface : PartialClass
    {
        public PartialInterface(IEnumerable<PartialReductionDeclaration> functions, IEnumerable<PartialParameterDeclaration> genericArgs) : base(functions, genericArgs) { }
    }
}
