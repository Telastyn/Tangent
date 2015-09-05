using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class SumType : TangentType
    {
        private readonly HashSet<TangentType> types;

        public IEnumerable<TangentType> Types
        {
            get
            {
                return types;
            }
        }

        private SumType(HashSet<TangentType> types)
            : base(KindOfType.Sum)
        {
            this.types = types;
        }

        private static readonly Dictionary<int, List<SumType>> creationCache = new Dictionary<int, List<SumType>>();

        public static SumType For(IEnumerable<TangentType> types)
        {
            HashSet<TangentType> set = new HashSet<TangentType>(types);
            List<SumType> existing;
            bool newb = false;
            if (creationCache.TryGetValue(set.Count, out existing)) {
                foreach (var entry in existing) {
                    if (entry.Types.SequenceEqual(set)) {
                        return entry;
                    }
                }
            } else {
                newb = true;
            }

            var result = new SumType(set);
            if (newb) {
                creationCache.Add(set.Count, new List<SumType>() { result });
            } else {
                creationCache[set.Count].Add(result);
            }

            return result;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            if (this == other) { return true; }

            return false;
            
            // RMS: right now, there's no anonymous sum types in parameters, so they can't be matched. Leaving this here in case they might.

            //var otherSum = other as SumType;
            //if (otherSum == null) { return false; }
            //if (this.types.Count != otherSum.types.Count) { return false; }
            //var leftOvers = this.Types.Except(otherSum.Types);
            //if (!leftOvers.Any()) { return false; }
            //if (!leftOvers.All(tt => tt.ImplementationType == KindOfType.InferencePoint)) { return false; }
            //if (leftOvers.Skip(1).Any()) { throw new InvalidOperationException("Trying to infer a sum type with multiple inference points."); }

            //var otherMatch = otherSum.Types.Except(this.Types).First();
            //var leftoverInference = leftOvers.First() as GenericInferencePlaceholder;
            //necessaryTypeInferences.Add(leftoverInference.GenericArgument, otherMatch);
            //return true;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return SumType.For(this.Types.Select(t => t.ResolveGenericReferences(mapping)));
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) { return Enumerable.Empty<ParameterDeclaration>(); }
            alreadyProcessed.Add(this);

            return this.Types.SelectMany(tt => tt.ContainedGenericReferences(tie,alreadyProcessed));
        }
    }
}
