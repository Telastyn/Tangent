using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing.Partial
{
    public class PartialProductType : PartialClass
    {
        public readonly List<PartialPhrasePart> DataConstructorParts;
        public readonly List<TangentType> InterfaceReferences;

        internal PartialProductType(IEnumerable<PartialPhrasePart> dataConstructorParts, IEnumerable<PartialReductionDeclaration> functions, IEnumerable<VarDeclElement> fields, IEnumerable<PartialDelegateDeclaration> delegates, IEnumerable<PartialParameterDeclaration> genericArgs, IEnumerable<TangentType> interfaceReferences)
            : base(functions, fields, delegates, genericArgs)
        {
            this.DataConstructorParts = new List<PartialPhrasePart>(dataConstructorParts);
            this.InterfaceReferences = new List<TangentType>(interfaceReferences);
        }
    }
}
