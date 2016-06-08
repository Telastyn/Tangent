using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class FieldAccessorFunction : Function
    {
        public readonly ProductType OwningType;
        public readonly Field TargetField;

        public FieldAccessorFunction(ProductType type, Field targetField) : base(targetField.Declaration.Returns, null)
        {
            OwningType = type;
            TargetField = targetField;
        }
    }
}
