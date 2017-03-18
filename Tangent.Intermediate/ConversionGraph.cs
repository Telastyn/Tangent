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
        private readonly List<TangentType> GenericSources = new List<TangentType>();
        private readonly Dictionary<TangentType, List<TangentType>> PartialGenericSources = new Dictionary<TangentType, List<TangentType>>();

        public ConversionGraph(IEnumerable<ReductionDeclaration> conversionOperations)
        {
            conversionOperations = conversionOperations.Where(fn => fn.IsConversion);
            foreach (var entry in conversionOperations) {
                var convertFromType = entry.Takes.First().Parameter.RequiredArgumentType;
                if (convertFromType.ContainedGenericReferences().Any()) {
                    var bgt = convertFromType as BoundGenericType;
                    if (bgt != null) {
                        if (!PartialGenericSources.ContainsKey(bgt.GenericType)) {
                            PartialGenericSources.Add(bgt.GenericType, new List<TangentType>());
                        }

                        PartialGenericSources[bgt.GenericType].Add(convertFromType);
                    } else {
                        GenericSources.Add(convertFromType);
                    }
                }
                if (!Paths.ContainsKey(convertFromType)) {
                    Paths.Add(convertFromType, new Dictionary<TangentType, ConversionPath>());
                }

                if (Paths[convertFromType].ContainsKey(entry.Returns.EffectiveType)) {
                    // Nothing for now? This comes up with .NET types and interfaces sometimes.
                } else {
                    Paths[convertFromType].Add(entry.Returns.EffectiveType, new ConversionPath(entry));
                }
            }

            BuildGraph();
        }

        public ConversionPath FindConversion(TangentType from, TangentType to)
        {
            // Converting to single values can't happen.
            if (to.ImplementationType == KindOfType.SingleValue) {
                return null;
            }

            var fromGeneric = from.ContainedGenericReferences();
            var toGeneric = to.ContainedGenericReferences();

            // Generic results must be based on generic inputs.
            if (toGeneric.Any(g => !fromGeneric.Contains(g))) { return null; }

            if (!fromGeneric.Any() && Paths.ContainsKey(from) && Paths[from].ContainsKey(to)) {
                return Paths[from][to];
            }

            ConversionPath conversion = null;
            // Check if we're doing something like int -> ~>int
            if (to.ImplementationType == KindOfType.Delegate) {
                var delegateType = (DelegateType)to;
                if (!delegateType.Takes.Any()) {
                    // Some lazy type.
                    if (delegateType.Returns == from) {
                        conversion = ConversionPath.Lazify(from);
                    } else {
                        var almostConversion = FindConversion(from, delegateType.Returns);
                        if (almostConversion != null) {
                            conversion = ConversionPath.Lazify(almostConversion);
                        }
                    }
                }
            }

            // if still nothing, check generic conversions.
            if (conversion == null) {
                var fromBgt = from as BoundGenericType;
                var genericCandidates = new List<Tuple<TangentType, ConversionPath>>();
                if (fromBgt != null && PartialGenericSources.ContainsKey(fromBgt.GenericType)) {
                    foreach(var generic in PartialGenericSources[fromBgt.GenericType]) {
                        var inferences = new Dictionary<ParameterDeclaration, TangentType>();
                        if (generic.CompatibilityMatches(from, inferences)) {
                            var goodTargets = Paths[generic].Where(kvp => kvp.Key.ResolveGenericReferences(pd => inferences[pd]) == to).Select(kvp => kvp.Value).ToList();
                            genericCandidates.AddRange(goodTargets.Select(t => Tuple.Create(generic, t)));
                        }
                    }
                }

                if (!genericCandidates.Any()) {
                    foreach (var generic in GenericSources) {
                        var inferences = new Dictionary<ParameterDeclaration, TangentType>();
                        if (generic.CompatibilityMatches(from, inferences)) {
                            var goodTargets = Paths[generic].Where(kvp => kvp.Key.ResolveGenericReferences(pd => inferences[pd]) == to).Select(kvp => kvp.Value).ToList();
                            genericCandidates.AddRange(goodTargets.Select(t => Tuple.Create(generic, t)));
                        }
                    }
                }

                if (genericCandidates.Count == 1) {
                    conversion = genericCandidates.First().Item2;
                } else if (genericCandidates.Count == 0) {
                    // leave it null.
                } else {
                    // We have many paths.
                    // Pick the ones with the most specific generic source. 
                    // Then the ones with the shortest conversion cost.
                    // If still multiple, toss ambiguity.
                    var sources = GetBestGenerics(new HashSet<TangentType>(genericCandidates.Select(t => t.Item1)));
                    genericCandidates = genericCandidates.Where(gc => sources.Contains(gc.Item1)).ToList();
                    if (genericCandidates.Count == 1) {
                        conversion = genericCandidates.First().Item2;
                    } else {
                        genericCandidates = genericCandidates.GroupBy(c => c.Item2.Cost).OrderBy(g => g.Key).First().ToList();
                        if (genericCandidates.Count == 1) {
                            conversion = genericCandidates.First().Item2;
                        } else {
                            // TODO: return all ambiguities.
                            conversion = ConversionPath.Ambiguity(genericCandidates.First().Item2, genericCandidates.Skip(1).First().Item2);
                        }
                    }
                }

                if (fromGeneric.Any()) {
                    return conversion;
                }

                if (!Paths.ContainsKey(from)) {
                    Paths.Add(from, new Dictionary<TangentType, ConversionPath>());
                }

                if (!Paths[from].ContainsKey(to)) {
                    Paths[from].Add(to, conversion);
                } else {
                    // TODO: do we need this? If we get here it's probably a bug?
                    Paths[from][to] = conversion;
                }
            }

            return Paths[from][to];
        }

        private void BuildGraph()
        {
            // Take all of our paths and fan them out, discovering multi-link paths.
            HashSet<TangentType> processedNodes = new HashSet<TangentType>();
            foreach (var t in Paths) {
                BuildGraphFor(t.Key, processedNodes);
            }
        }

        private void BuildGraphFor(TangentType t, HashSet<TangentType> processedNodes)
        {
            if (processedNodes.Contains(t)) { return; }
            processedNodes.Add(t);
            if (Paths.ContainsKey(t)) {
                List<Tuple<TangentType, ConversionPath>> newbs = new List<Tuple<TangentType, ConversionPath>>();
                foreach (var path in Paths[t]) {
                    var endpoint = path.Key;
                    var endpointGenerics = endpoint.ContainedGenericReferences();
                    if (endpointGenerics.Any()) {
                        foreach (var generic in GenericSources.Where(gs => gs != t)) {
                            var inferences = new Dictionary<ParameterDeclaration, TangentType>();
                            if (generic.CompatibilityMatches(endpoint, inferences)) {
                                BuildGraphFor(generic, processedNodes);
                                if (Paths.ContainsKey(generic)) {
                                    foreach (var subPath in Paths[generic]) {
                                        var newTo = subPath.Key.ResolveGenericReferences(pd => inferences[pd]);
                                        newbs.Add(Tuple.Create(newTo, subPath.Value));
                                    }
                                }
                            }
                        }
                    } else {
                        BuildGraphFor(endpoint, processedNodes);
                        if (Paths.ContainsKey(endpoint)) {
                            foreach (var subPath in Paths[endpoint]) {
                                newbs.Add(Tuple.Create(subPath.Key, new ConversionPath(path.Value, subPath.Value)));
                            }
                        }
                    }
                }

                foreach (var entry in newbs) {
                    if (!Paths[t].ContainsKey(entry.Item1)) {
                        Paths[t].Add(entry.Item1, entry.Item2);
                    } else {
                        var existing = Paths[t][entry.Item1];
                        var newb = entry.Item2;
                        if (existing.IsGeneric && !newb.IsGeneric) {
                            Paths[t][entry.Item1] = newb;
                        } else if (!existing.IsGeneric && newb.IsGeneric) {
                            // Leave it.
                        } else if (existing.Cost < newb.Cost) {
                            // Leave it.
                        } else if (existing.Cost > newb.Cost) {
                            Paths[t][entry.Item1] = newb;
                        } else {
                            Paths[t][entry.Item1] = ConversionPath.Ambiguity(existing, newb);
                        }
                    }
                }
            }
        }

        private HashSet<TangentType> GetBestGenerics(HashSet<TangentType> candidates)
        {
            if (candidates.Count <= 1) { return candidates; }
            var best = new HashSet<TangentType>();
            foreach (var candidate in candidates) {
                if (!best.Any()) {
                    best.Add(candidate);
                } else {
                    var cmp = PhrasePriorityComparer.CompareGeneric(candidate, best.First());
                    if (cmp > 0) {
                        best = new HashSet<TangentType>() { candidate };
                    } else if (cmp == 0) {
                        best.Add(candidate);
                    } // else, nothing.
                }
            }

            return best;
        }
    }
}
