using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class ReductionDeclaration {
        public readonly Identifier Takes;
        public readonly FunctionOrReductionDeclaration Returns;

        public ReductionDeclaration(Identifier takes, FunctionOrReductionDeclaration returns) {
            Takes = takes;
            Returns = returns;
        }
    }
}
