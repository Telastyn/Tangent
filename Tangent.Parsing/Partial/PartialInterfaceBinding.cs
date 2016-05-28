using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialInterfaceBinding
    {
        public readonly List<PartialPhrasePart> TypePhrase;
        public readonly List<TangentType> InterfaceReferences;
        public readonly List<PartialReductionDeclaration> Functions;

        public PartialInterfaceBinding(IEnumerable<PartialPhrasePart> type, IEnumerable<TangentType> interfaceReferences, IEnumerable<PartialReductionDeclaration> functions = null)
        {
            TypePhrase = new List<PartialPhrasePart>(type ?? Enumerable.Empty<PartialPhrasePart>());
            InterfaceReferences = new List<TangentType>(interfaceReferences ?? Enumerable.Empty<TangentType>());
            if (!InterfaceReferences.Any()) { throw new InvalidOperationException("Somehow interface binding got created with no interfaces..."); }
            Functions = new List<PartialReductionDeclaration>(functions ?? Enumerable.Empty<PartialReductionDeclaration>());
        }
    }
}
