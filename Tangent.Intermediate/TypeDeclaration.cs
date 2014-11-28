using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class TypeDeclaration {
        public readonly Identifier Takes;
        public readonly TypeOrTypeDeclaration Returns;

        public TypeDeclaration(Identifier takes, TypeOrTypeDeclaration returns) {
            Takes = takes;
            Returns = returns;
        }
    }
}
