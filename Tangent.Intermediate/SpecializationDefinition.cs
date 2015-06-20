using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class SpecializationDefinition
    {
        public readonly IEnumerable<SpecializationEntry> Specializations;

        public SpecializationDefinition(IEnumerable<SpecializationEntry> entries)
        {
            this.Specializations = entries;
        }
    }
}
