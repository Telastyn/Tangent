﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ParameterDeclaration : ReductionRule<PhrasePart, TangentType>
    {
        public ParameterDeclaration(Identifier takes, TangentType returns) : this(new[] { takes }, returns) { }
        public ParameterDeclaration(IEnumerable<Identifier> takes, TangentType returns) : this(takes.Select(t => new PhrasePart(t)), returns) { }
        public ParameterDeclaration(IEnumerable<PhrasePart> takes, TangentType returns) : base(takes, returns) { }

        public override string SeparatorToken
        {
            get { return ":"; }
        }

        public readonly Guid Tracer = Guid.NewGuid();

        private TangentType requiredArgumentType = null;
        public TangentType RequiredArgumentType
        {
            get
            {
                requiredArgumentType = requiredArgumentType ?? BuildRequiredArgumentType();
                return requiredArgumentType;
            }
        }

        private TangentType BuildRequiredArgumentType()
        {
            var delegateParams = Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter.Returns);
            if (delegateParams.Any()) {
                return DelegateType.For(delegateParams, Returns);
            } else {
                return Returns;
            }
        }

        public bool IsThisParam
        {
            get
            {
                return Takes.Count == 1 && Takes[0].IsIdentifier && Takes[0].Identifier == "this";
            }
        }

        public override string ToString()
        {
            if (Returns == TangentType.Any.Kind) {
                return string.Format("({0})", string.Join(" ", Takes));
            }

            return string.Format("({0})", base.ToString());
        }

        public ParameterDeclaration ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return new ParameterDeclaration(Takes.Select(pp => pp.ResolveGenericReferences(mapping)), Returns.ResolveGenericReferences(mapping));
        }
    }
}
