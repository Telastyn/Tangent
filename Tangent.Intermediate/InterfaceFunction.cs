using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    // A concrete implementation for interface bases, which serves as a placeholder for dispatch. 
    public class InterfaceFunction : Function
    {
        public readonly TypeClass SourceInterface;
        public InterfaceFunction(TypeClass forInterface, TangentType returnType) : base(returnType, new Block(Enumerable.Empty<Expression>()))
        {
            SourceInterface = forInterface;
        }
    }
}
