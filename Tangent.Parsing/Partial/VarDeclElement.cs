using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class VarDeclElement : PartialElement
    {
        public readonly PartialParameterDeclaration ParameterDeclaration;
        public readonly PartialStatement Initializer;

        public VarDeclElement(PartialParameterDeclaration decl, PartialStatement initializer, LineColumnRange sourceInfo)
            : base(ElementType.VarDecl, sourceInfo)
        {
            Initializer = initializer;
            ParameterDeclaration = decl;
        }
    }
}
