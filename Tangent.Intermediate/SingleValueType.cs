﻿using System;
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

        public SingleValueType(EnumType valueType, Identifier value)
            : base(KindOfType.SingleValue)
        {
            if (!valueType.Values.Contains(value))
            {
                throw new InvalidOperationException();
            }

            ValueType = valueType;
            Value = value;
        }
    }
}
