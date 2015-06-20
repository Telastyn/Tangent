using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Errors
{
    public class ThisAsGeneric:ParseError
    {
        public ThisAsGeneric() : base() { }
        public override string ToString()
        {
            return "'this' may not be used as a generic parameter name.";
        }
    }
}
