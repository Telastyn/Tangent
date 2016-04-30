using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate {
    public class DirectOpCode : Function {
        public readonly OpCode OpCode;
        internal DirectOpCode(OpCode code, TangentType type)
            : base(type, null) {
            this.OpCode = code;
        }
    }
}
