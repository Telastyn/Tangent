using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public class Scope
    {
        public readonly IEnumerable<ParameterDeclaration> Parameters;
        public readonly IEnumerable<TypeDeclaration> Types;
        public readonly IEnumerable<ReductionDeclaration> Functions;
        public readonly IEnumerable<ParameterDeclaration> CtorParameters;
        public readonly TangentType ReturnType;

        public Scope(TangentType returnType, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> parameters, IEnumerable<ParameterDeclaration> ctorParameters, IEnumerable<ReductionDeclaration> functions)
        {
            ReturnType = returnType;
            Parameters = parameters.OrderByDescending(p => p.Takes.Count()).ToList();
            CtorParameters = ctorParameters.OrderByDescending(p => p.Takes.Count()).ToList();
            Types = types.OrderByDescending(t => t.Takes.Count()).ToList();
            Functions = (functions.OrderByDescending(f => f.Takes.Count())).ToList();
        }

        public static Scope ForTypes(IEnumerable<TypeDeclaration> types)
        {
            return new Scope(TangentType.Any.Kind, types, Enumerable.Empty<ParameterDeclaration>(), Enumerable.Empty<ParameterDeclaration>(), Enumerable.Empty<ReductionDeclaration>());
        }
    }
}
