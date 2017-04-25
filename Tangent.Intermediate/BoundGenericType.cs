using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class BoundGenericType : TangentType
    {
        public readonly TangentType GenericType;
        public readonly IEnumerable<TangentType> TypeArguments;

        private static readonly List<BoundGenericType> concreteTypes = new List<BoundGenericType>();

        private BoundGenericType(TangentType generic, IEnumerable<TangentType> arguments)
            : base(KindOfType.BoundGeneric)
        {
            if (generic == null) { throw new InvalidOperationException(); }
            GenericType = generic;
            TypeArguments = arguments;
        }

        public static BoundGenericType For(HasGenericParameters generic, IEnumerable<TangentType> arguments)
        {
            if (generic == null) { throw new ArgumentNullException("generic"); }
            if (arguments.Count() != generic.GenericParameters.Count()) { throw new InvalidOperationException(); }

            var result = concreteTypes.FirstOrDefault(t => t.GenericType == generic && t.TypeArguments.SequenceEqual(arguments));
            if (result != null) {
                return result;
            }

            result = new BoundGenericType(generic as TangentType, arguments);
            concreteTypes.Add(result);
            return result;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            var boundOther = other as BoundGenericType;
            if (boundOther == null) { return false; }
            if (boundOther.GenericType != this.GenericType) { return false; }
            foreach (var pair in this.TypeArguments.Zip(boundOther.TypeArguments, (a, b) => Tuple.Create(a, b))) {
                var gat = pair.Item1 as GenericArgumentReferenceType;
                if (gat != null) {
                    if (!necessaryTypeInferences.ContainsKey(gat.GenericParameter)) {
                        necessaryTypeInferences[gat.GenericParameter] = pair.Item2;
                    } else if (necessaryTypeInferences[gat.GenericParameter] != pair.Item2) {
                        // See GenericArgumentReference type. Something is weird. Return for now.
                        return false;
                    }
                }

                if (!pair.Item1.CompatibilityMatches(pair.Item2, necessaryTypeInferences)) {
                    return false;
                }
            }

            return true;
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) { return Enumerable.Empty<ParameterDeclaration>(); }
            alreadyProcessed.Add(this);
            return TypeArguments.SelectMany(t => t.ContainedGenericReferences(alreadyProcessed));
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return Intermediate.BoundGenericType.For((HasGenericParameters)this.GenericType, this.TypeArguments.Select(t => t.ResolveGenericReferences(mapping)));
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}>", GenericType.ToString(), string.Join(", ", TypeArguments.Select(ta => ta.ToString())));
        }
    }
}
