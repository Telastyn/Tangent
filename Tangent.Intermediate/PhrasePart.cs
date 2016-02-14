using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class PhrasePart
    {
        public readonly Identifier Identifier;
        public readonly ParameterDeclaration Parameter;
        public bool IsIdentifier { get { return Identifier != null; } }

        public PhrasePart(Identifier id)
        {
            Identifier = id;
        }

        public PhrasePart(ParameterDeclaration decl)
        {
            Parameter = decl;
        }

        public static implicit operator PhrasePart(Identifier id)
        {
            return new PhrasePart(id);
        }

        public static implicit operator PhrasePart(ParameterDeclaration decl)
        {
            return new PhrasePart(decl);
        }

        public override string ToString()
        {
            if (IsIdentifier) { return Identifier.Value; }
            return Parameter.ToString();
        }

        public PhrasePart ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            if (IsIdentifier) {
                return this;
            }

            return new PhrasePart(Parameter.ResolveGenericReferences(mapping));
        }
    }
}
