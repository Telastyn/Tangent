using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public enum DispatchType
    {
        SingleValue,
        SumType,
        GenericSpecialization
    }

    public class SpecializationEntry
    {
        public readonly ParameterDeclaration GeneralFunctionParameter;
        public readonly ParameterDeclaration SpecificFunctionParameter;
        public readonly Dictionary<ParameterDeclaration, TangentType> InferenceSpecializations = null;

        public DispatchType SpecializationType
        {
            get
            {
                if (InferenceSpecializations != null) {
                    return DispatchType.GenericSpecialization;
                }

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

        public SpecializationEntry(ParameterDeclaration general, ParameterDeclaration specific, Dictionary<ParameterDeclaration, TangentType> inferences = null)
        {
            GeneralFunctionParameter = general;
            SpecificFunctionParameter = specific;
            if (inferences != null && !general.Returns.ContainedGenericReferences(GenericTie.Inference).All(pd => inferences.ContainsKey(pd))) {
                throw new InvalidOperationException("Specialization Entry being created with inferences that do not match general version.");
            }

            InferenceSpecializations = inferences;
        }
    }
}
