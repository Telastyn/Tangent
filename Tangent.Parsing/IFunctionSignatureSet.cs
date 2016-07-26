using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public interface IFunctionSignatureSet
    {
        List<ReductionDeclaration> Functions { get; }
        void Add(ReductionDeclaration fn);
    }

    public static class ExtendFunctionSignatureSet
    {
        public static void AddRange(this IFunctionSignatureSet set, IEnumerable<ReductionDeclaration> fns)
        {
            foreach (var entry in fns) {
                set.Add(entry);
            }
        }
    }
}
