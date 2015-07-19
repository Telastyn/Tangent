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
        public VarDeclElement(PartialParameterDeclaration decl, LineColumnRange sourceInfo)
            : base(ElementType.VarDecl, sourceInfo)
        {
            ParameterDeclaration = decl;
        }
    }
}
