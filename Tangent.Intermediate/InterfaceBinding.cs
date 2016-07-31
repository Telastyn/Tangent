using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate {
    public class InterfaceBinding {
        public readonly TypeClass Interface;
        public readonly TangentType Implementation;

        public InterfaceBinding(TypeClass iface, TangentType impl) {
            Interface = iface;
            Implementation = impl;

            // gross.
            Interface.Implementations.Add(Implementation);

            // TODO: what if impl is another interface? 
        }
    }
}
