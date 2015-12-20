using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class DelegateType : TangentType
    {
        public readonly List<TangentType> Takes;
        public readonly TangentType Returns;

        private DelegateType(IEnumerable<TangentType> takes, TangentType returns)
            : base(KindOfType.Delegate)
        {
            Takes = new List<TangentType>(takes);
            Returns = returns;
        }

        private static Dictionary<TangentType, Dictionary<IEnumerable<TangentType>, DelegateType>> cache = new Dictionary<TangentType, Dictionary<IEnumerable<TangentType>, DelegateType>>();

        public static DelegateType For(IEnumerable<TangentType> takes, TangentType returns)
        {
            if (!takes.Any()) {
                throw new InvalidOperationException("Use lazy types for nullary delegates.");
            }

            lock (cache) {
                if (!cache.ContainsKey(returns)) {
                    var result = new DelegateType(takes, returns);
                    cache.Add(returns, new Dictionary<IEnumerable<TangentType>, DelegateType>() { { takes, result } });
                    return result;
                }

                foreach (var entry in cache[returns]) {
                    if (entry.Key.SequenceEqual(takes)) {
                        return entry.Value;
                    }
                }

                var newb = new DelegateType(takes, returns);
                cache[returns].Add(takes, newb);
                return newb;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} => {1}", Takes.Select(t => string.Format("({0})", Takes)), Returns);
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            var newTakes = Takes.Select(t => t.ResolveGenericReferences(mapping));
            var newReturns = Returns.ResolveGenericReferences(mapping);
            return For(newTakes, newReturns);
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(GenericTie tie, HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) {
                return Enumerable.Empty<ParameterDeclaration>();
            }

            alreadyProcessed.Add(this);
            var result = Takes.Aggregate(Enumerable.Empty<ParameterDeclaration>(), (a, t) => a.Concat(t.ContainedGenericReferences(tie, alreadyProcessed))).ToList();
            result.AddRange(Returns.ContainedGenericReferences(tie, alreadyProcessed));
            return result;
        }

        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            var delegateOther = other as DelegateType;
            if (delegateOther == null) {
                return false;
            }

            if (delegateOther.Takes.Count != this.Takes.Count) {
                return false;
            }

            return this.Takes.Zip(delegateOther.Takes, (a, b) => a.CompatibilityMatches(b, necessaryTypeInferences)).Aggregate((a, b) => a & b)
                && this.Returns.CompatibilityMatches(delegateOther.Returns, necessaryTypeInferences);
        }
    }
}
