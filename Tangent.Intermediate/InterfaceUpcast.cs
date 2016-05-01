using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate {
    public class InterfaceUpcast : Function {
        public TypeClass TargetInterface { get { return EffectiveType as TypeClass; } }
        public InterfaceUpcast(TypeClass targetInterface) : base(targetInterface, null) { }
        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset) {
            // nada.
        }
    }
}
