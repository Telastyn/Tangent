using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ProductType : TangentType
    {
        public readonly List<PhrasePart> DataConstructorParts;
        
        public ProductType(IEnumerable<PhrasePart> dataConstructorParts)
            : base(KindOfType.Product)
        {
            this.DataConstructorParts = new List<PhrasePart>(dataConstructorParts);
        }
    }
}
