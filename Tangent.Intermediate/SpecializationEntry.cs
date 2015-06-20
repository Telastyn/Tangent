using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public enum DispatchType{
        SingleValue,
        SumType
    }

    public class SpecializationEntry
    {
        public readonly ParameterDeclaration GeneralFunctionParameter;
        public readonly ParameterDeclaration SpecificFunctionParameter;

        public DispatchType SpecializationType
        {
            get
            {
                switch (GeneralFunctionParameter.Returns.ImplementationType) {
                    case KindOfType.Sum:
                        return DispatchType.SumType;
                    case KindOfType.Enum:
                        return DispatchType.SingleValue;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public SpecializationEntry(ParameterDeclaration general, ParameterDeclaration specific)
        {
            GeneralFunctionParameter = general;
            SpecificFunctionParameter = specific;
        }
    }
}
