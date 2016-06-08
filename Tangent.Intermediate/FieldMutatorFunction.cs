using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldMutatorFunction:Function
    {
        public readonly ProductType OwningType;
        public readonly Field TargetField;

        public FieldMutatorFunction(ProductType type, Field targetField):base(TangentType.Void, null)
        {
            OwningType = type;
            TargetField = targetField;
        }
    }
}
