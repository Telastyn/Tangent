using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public class LambdaElement : PartialElement
    {
        public readonly List<VarDeclElement> Takes;
        public readonly BlockElement Body;

        public LambdaElement(List<VarDeclElement> takes, BlockElement body)
            : base(ElementType.Lambda, LineColumnRange.CombineAll(takes.Select(t => t.SourceInfo).Concat(new[] { body.SourceInfo })))
        {
            //if (takes.Any(t => t.ParameterDeclaration.Returns != null)) {
            //    throw new InvalidOperationException("Typed delegate parameters are not currently supported.");
            //}

            Takes = takes;
            Body = body;
        }
    }
}
