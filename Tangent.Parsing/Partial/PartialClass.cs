﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Parsing.Partial
{
    public abstract class PartialClass : PlaceholderType
    {
        public readonly List<PartialReductionDeclaration> Functions;
        public readonly List<VarDeclElement> Fields;
        public readonly List<PartialParameterDeclaration> GenericArguments;

        public PartialClass(IEnumerable<PartialReductionDeclaration> functions, IEnumerable<VarDeclElement> fields, IEnumerable<PartialParameterDeclaration> genericArgs)
            : base()
        {
            this.Functions = new List<PartialReductionDeclaration>(functions);
            this.Fields = new List<VarDeclElement>(fields);
            this.GenericArguments = new List<PartialParameterDeclaration>(genericArgs);
        }
    }
}
