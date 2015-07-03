using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialProductType : PlaceholderType
    {
        public readonly List<PartialPhrasePart> DataConstructorParts;
        public readonly List<PartialReductionDeclaration> Functions;
        public readonly List<PartialParameterDeclaration> GenericArguments;

        internal PartialProductType(IEnumerable<PartialPhrasePart> dataConstructorParts, IEnumerable<PartialReductionDeclaration> functions, IEnumerable<PartialParameterDeclaration> genericArgs)
            : base()
        {
            this.DataConstructorParts = new List<PartialPhrasePart>(dataConstructorParts);
            this.Functions = new List<PartialReductionDeclaration>(functions);
            this.GenericArguments = new List<PartialParameterDeclaration>(genericArgs);
        }
    }
}
