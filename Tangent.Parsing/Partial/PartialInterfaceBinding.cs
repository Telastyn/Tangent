using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialInterfaceBinding
    {
        public readonly List<PartialPhrasePart> TypePhrase;
        public readonly List<PartialPhrasePart> TypeClassPhrase;
        public readonly List<PartialReductionDeclaration> Functions;

        public PartialInterfaceBinding(List<PartialPhrasePart> type, List<PartialPhrasePart> typeClass, List<PartialReductionDeclaration> functions = null)
        {
            TypePhrase = type ?? new List<PartialPhrasePart>();
            TypeClassPhrase = typeClass ?? new List<PartialPhrasePart>();
            Functions = functions ?? new List<PartialReductionDeclaration>();
        }
    }
}
