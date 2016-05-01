using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate {
    // A concrete implementation for interface bases, which serves as a placeholder for dispatch. 
    public class InterfaceFunction : Function {
        public InterfaceFunction(TypeClass forInterface) : base(forInterface, new Block(Enumerable.Empty<Expression>())) {

        }
    }
}
