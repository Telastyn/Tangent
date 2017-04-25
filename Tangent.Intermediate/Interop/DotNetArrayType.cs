using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    /// <summary>
    /// PsuedoGeneric class to represent Array<T> to Tangent interop.
    /// </summary>
    public class DotNetArrayType : TangentType, HasGenericParameters
    {
        public static readonly DotNetArrayType Common = new DotNetArrayType();
        private static ParameterDeclaration typeParameter;
        public static ParameterDeclaration TypeParameter
        {
            get
            {
                if (typeParameter == null) {
                    typeParameter = new ParameterDeclaration("T", TangentType.Any.Kind);
                }

                return typeParameter;
            }
        }

        private DotNetArrayType() : base(KindOfType.Builtin) { }

        public IEnumerable<ParameterDeclaration> GenericParameters
        {
            get
            {
                yield return TypeParameter;
            }
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return other is DotNetArrayType;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override string ToString()
        {
            return "Array";
        }
    }
}
