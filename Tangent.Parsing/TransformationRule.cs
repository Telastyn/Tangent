using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public abstract class TransformationRule
    {
        public abstract TransformationResult TryReduce(List<Expression> buffer);
    }
}
