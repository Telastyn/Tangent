using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class Field
    {
        public readonly ParameterDeclaration Declaration;
        public readonly Expression Initializer;

        public Field(ParameterDeclaration decl, Expression initializer)
        {
            Declaration = decl;
            Initializer = initializer;
        }
    }
}
