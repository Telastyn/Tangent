using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class PartialDelegateDeclaration
    {
        public readonly IEnumerable<PartialPhrasePart> FieldPart;
        public readonly IEnumerable<PartialPhrasePart> FunctionPart;
        public readonly PartialFunction DefaultImplementation;

        public PartialDelegateDeclaration(IEnumerable<PartialPhrasePart> fieldPart, IEnumerable<PartialPhrasePart> functionPart, PartialFunction defaultImpl)
        {
            FieldPart = fieldPart;
            FunctionPart = functionPart;
            DefaultImplementation = defaultImpl;
        }
    }
}
