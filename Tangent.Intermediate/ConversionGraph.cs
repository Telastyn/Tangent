using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ConversionGraph
    {
        private readonly Dictionary<TangentType, Dictionary<TangentType, ConversionPath>> Paths = new Dictionary<TangentType, Dictionary<TangentType, ConversionPath>>();
        private readonly Dictionary<ReductionDeclaration, Dictionary<TangentType, Dictionary<TangentType, ConversionPath>>> GenericPaths = new Dictionary<ReductionDeclaration, Dictionary<TangentType, Dictionary<TangentType, ConversionPath>>>();

        public ConversionGraph(IEnumerable<ReductionDeclaration> conversionOperations)
        {
            conversionOperations = conversionOperations.Where(fn => fn.IsConversion);
            foreach (var entry in conversionOperations) {
                if (entry.GenericParameters.Any()) {
                    GenericPaths.Add(entry, new Dictionary<TangentType, Dictionary<TangentType, ConversionPath>>());
                } else {
                    var convertFromType = entry.Takes.First().Parameter.RequiredArgumentType;
                    if (!Paths.ContainsKey(convertFromType)) {
                        Paths.Add(convertFromType, new Dictionary<TangentType, ConversionPath>());
                    }

                    if (Paths[convertFromType].ContainsKey(entry.Returns.EffectiveType)) {
                        // Nothing for now? This comes up with .NET types and interfaces sometimes.
                    } else {
                        Paths[convertFromType].Add(entry.Returns.EffectiveType, new ConversionPath(entry));
                    }
                }
            }
        }

        public ConversionPath FindConversion(TangentType from, TangentType to)
        {
            if (to.ImplementationType == KindOfType.SingleValue) {
                return null;
            }

            if (!Paths.ContainsKey(from) || !Paths[from].ContainsKey(to)) {
                PathFind(from, to);
            }

            if (!Paths.ContainsKey(from) || !Paths[from].ContainsKey(to)) {
                ConversionPath conversion = null;
                if (to.ImplementationType == KindOfType.Delegate) {
                    var delegateType = (DelegateType)to;
                    if (!delegateType.Takes.Any()) {
                        // Some lazy type.
                        if (delegateType.Returns == from) {
                            conversion = ConversionPath.Lazify(from);
                        } else {
                            PathFind(from, delegateType.Returns);
                            if (Paths.ContainsKey(from) && Paths[from].ContainsKey(delegateType.Returns)) {
                                var almostLazyPath = Paths[from][delegateType.Returns];
                                if (almostLazyPath != null) {
                                    conversion = ConversionPath.Lazify(almostLazyPath);
                                }
                            }
                        }
                    }
                }

                if (!Paths.ContainsKey(from)) {
                    Paths.Add(from, new Dictionary<TangentType, ConversionPath>());
                }

                if (!Paths[from].ContainsKey(to)) {
                    Paths[from].Add(to, conversion);
                } else {
                    Paths[from][to] = conversion;
                }
            }

            return Paths[from][to];
        }

        private void PathFind(TangentType from, TangentType to)
        {
            // We have some conversion we want, but don't yet know how to get from A to B.
            Dictionary<TangentType, ConversionPath> candidates = new Dictionary<TangentType, ConversionPath>();

            if (Paths.ContainsKey(from)) {
                candidates = new Dictionary<TangentType, ConversionPath>(Paths[from]);
            }

            foreach (var entry in GenericPaths) {
                if (!entry.Value.ContainsKey(from)) {
                    // We haven't generated the generic conversions for the from type.
                    var inferences = new Dictionary<ParameterDeclaration, TangentType>();
                    if (entry.Key.Takes.First().Parameter.RequiredArgumentType.CompatibilityMatches(from, inferences)) {
                        var genericTo = entry.Key.Returns.EffectiveType.ResolveGenericReferences(pd => inferences[pd]);
                        entry.Value.Add(from, new Dictionary<TangentType, ConversionPath>() { { genericTo, new ConversionPath(entry.Key) } });
                        if (Paths.ContainsKey(from)) {
                            if (!Paths[from].ContainsKey(genericTo)) {
                                Paths[from].Add(genericTo, entry.Value[from][genericTo]);
                                candidates.Add(genericTo, entry.Value[from][to]);
                            } else {
                                // nada.
                            }
                        } else {
                            Paths.Add(from, new Dictionary<TangentType, ConversionPath>(entry.Value[from]));
                            candidates.Add(genericTo, entry.Value[from][to]);
                        }
                    } else {
                        entry.Value.Add(from, null);
                    }
                }
            }

            HashSet<TangentType> searchedTypes = new HashSet<TangentType>() { from };
            while (candidates.Select(kvp => kvp.Value != null).Any()) {
                if (candidates.ContainsKey(to)) {
                    return;
                }

                Dictionary<TangentType, ConversionPath> workset = new Dictionary<TangentType, ConversionPath>(candidates.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                searchedTypes.UnionWith(candidates.Keys);
                candidates = new Dictionary<TangentType, ConversionPath>();
                foreach (var entry in workset) {
                    if (Paths.ContainsKey(entry.Key)) {
                        foreach (var conversion in Paths[entry.Key].Where(kvp => kvp.Value != null)) {
                            if (!searchedTypes.Contains(conversion.Key)) {
                                var newConversion = new ConversionPath(entry.Value, conversion.Value);
                                AddBestPath(Paths[from], conversion.Key, newConversion);
                                AddBestPath(candidates, conversion.Key, newConversion);
                            }
                        }
                    } else {
                        // nada.
                    }

                    // TODO: generics
                }
            }
        }

        private void AddBestPath(Dictionary<TangentType, ConversionPath> lookup, TangentType to, ConversionPath path)
        {
            if (lookup.ContainsKey(to)) {
                var existing = lookup[to];

                if (existing.Cost < path.Cost || (!existing.IsGeneric && path.IsGeneric)) {
                    // leave better path.
                } else if (existing.Cost > path.Cost || (existing.IsGeneric && !path.IsGeneric)) {
                    lookup[to] = path;
                } else {
                    lookup[to] = ConversionPath.Ambiguity(existing, path);
                }
            } else {
                lookup.Add(to, path);
            }
        }
    }
}
