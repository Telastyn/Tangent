using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class TypeClass : TangentType, HasGenericParameters
    {
        public IEnumerable<ParameterDeclaration> GenericParameters { get; private set; }
        public readonly IEnumerable<ReductionDeclaration> RequiredFunctions;
        public readonly ParameterDeclaration ThisBindingInRequiredFunctions;
        public readonly List<TangentType> Implementations = new List<TangentType>();

        public TypeClass(IEnumerable<ReductionDeclaration> components)
            : base(KindOfType.TypeClass)
        {
            RequiredFunctions = new List<ReductionDeclaration>(components);
            ThisBindingInRequiredFunctions = new ParameterDeclaration("this", this.Kind);
            GenericParameters = new List<ParameterDeclaration>();
        }

        public bool IsSatisfiedBy(TangentType target, Func<ParameterDeclaration, TangentType> genericBindings, IEnumerable<ReductionDeclaration> functionPool)
        {
            // What we need is for each of our RequiredFunctions, some ReductionDeclaration in the functionPool that matches its signature when
            //  generics are suitably replaced.

            // TODO: handle compatible, but unequal parameters, such as different generic inferences with the same constraints.
            foreach (var fn in RequiredFunctions) {
                var fnRtn = fn.Returns.EffectiveType.ResolveGenericReferences(pd => pd == ThisBindingInRequiredFunctions ? target : genericBindings(pd));
                var fnPhrase = fn.Takes.Select(pp => pp.ResolveGenericReferences(pd => pd == ThisBindingInRequiredFunctions ? target : genericBindings(pd))).ToList();
                if (!functionPool.Any(targetFn => {
                    if (targetFn.Returns.EffectiveType != fnRtn) { return false; }
                    if (fnPhrase.Count != targetFn.Takes.Count) { return false; }
                    foreach (var pair in fnPhrase.Zip(targetFn.Takes, (a, b) => new { required = a, target = b })) {
                        if (pair.required.IsIdentifier != pair.target.IsIdentifier) { return false; }
                        if (pair.required.IsIdentifier && (pair.required.Identifier != pair.target.Identifier)) { return false; }
                        if (pair.required.Parameter.RequiredArgumentType != pair.target.Parameter.RequiredArgumentType) { return false; }
                    }

                    return true;
                })) {
                    return false;
                }
            }

            return true;
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            yield break;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) {
                return true;
            }

            if (other.ImplementationType == KindOfType.GenericReference) {
                var gart = other as GenericArgumentReferenceType;
                var constraint = ((KindType)gart.GenericParameter.Returns).KindOf;
                return this == constraint;
            }

            return false;
        }
    }
}
