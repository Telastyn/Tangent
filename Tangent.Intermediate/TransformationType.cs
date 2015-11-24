using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public enum TransformationType
    {
        BuiltIn = 1,
        LocalVariable = 2,
        GenericParam = 3,
        FunctionParameter = 4,
        ConstructorReference = 5,
        Type = 6,
        Function = 7,
        EnumValue = 8,
        Coersion = 9 
    }
}
