using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class SingleValueType : TangentType
    {

        public readonly EnumType ValueType;
        public readonly Identifier Value;
        public readonly int NumericEquivalent;

        internal SingleValueType(EnumType valueType, Identifier value, int numeric)
            : base(KindOfType.SingleValue)
        {
            if (!valueType.Values.Contains(value))
            {
                throw new InvalidOperationException();
            }

            ValueType = valueType;
            Value = value;
            NumericEquivalent = numeric;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }
    }
}
