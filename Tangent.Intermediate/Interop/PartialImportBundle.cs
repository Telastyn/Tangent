using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    public class PartialImportBundle
    {
        public Dictionary<Type, TypeDeclaration> Types = new Dictionary<Type, TypeDeclaration>();
        public Dictionary<MethodInfo, ReductionDeclaration> CommonFunctions = new Dictionary<MethodInfo, ReductionDeclaration>();
        public Dictionary<Type, Dictionary<Type, InterfaceBinding>> InterfaceBindings = new Dictionary<Type, Dictionary<Type, InterfaceBinding>>();
        public Dictionary<FieldInfo, ReductionDeclaration> FieldAccessors = new Dictionary<FieldInfo, ReductionDeclaration>();
        public Dictionary<FieldInfo, ReductionDeclaration> FieldMutators = new Dictionary<FieldInfo, ReductionDeclaration>();
        public Dictionary<ConstructorInfo, ReductionDeclaration> Constructors = new Dictionary<ConstructorInfo, ReductionDeclaration>();
        public Dictionary<Type, ReductionDeclaration> StructInits = new Dictionary<Type, ReductionDeclaration>();
        public Dictionary<Type, List<ReductionDeclaration>> SubtypingConversions = new Dictionary<Type, List<ReductionDeclaration>>();

        public static implicit operator ImportBundle(PartialImportBundle partial)
        {
            return new ImportBundle(partial.Types.Values, partial.CommonFunctions.Values.Concat(partial.FieldAccessors.Values).Concat(partial.FieldMutators.Values).Concat(partial.Constructors.Values).Concat(partial.StructInits.Values).Concat(partial.SubtypingConversions.Values.SelectMany(x=>x)), partial.InterfaceBindings.Values.SelectMany(x => x.Values));
        }

        public static PartialImportBundle Merge(PartialImportBundle a, PartialImportBundle b)
        {
            PartialImportBundle result = new PartialImportBundle();
            foreach (var entry in a.Types) {
                result.Types.Add(entry.Key, entry.Value);
            }

            foreach (var entry in a.CommonFunctions) {
                result.CommonFunctions.Add(entry.Key, entry.Value);
            }

            foreach (var entry in a.FieldAccessors) {
                result.FieldAccessors.Add(entry.Key, entry.Value);
            }

            foreach (var entry in a.FieldMutators) {
                result.FieldMutators.Add(entry.Key, entry.Value);
            }

            foreach (var entry in a.Constructors) {
                result.Constructors.Add(entry.Key, entry.Value);
            }

            foreach(var entry in a.StructInits) {
                result.StructInits.Add(entry.Key, entry.Value);
            }

            foreach (var entry in a.InterfaceBindings) {
                result.InterfaceBindings.Add(entry.Key, new Dictionary<Type, InterfaceBinding>());
                foreach (var sub in entry.Value) {
                    result.InterfaceBindings[entry.Key].Add(sub.Key, sub.Value);
                }
            }

            foreach(var entry in a.SubtypingConversions) {
                result.SubtypingConversions.Add(entry.Key, new List<ReductionDeclaration>());
                result.SubtypingConversions[entry.Key].AddRange(a.SubtypingConversions[entry.Key]);
            }

            foreach (var entry in b.Types) {
                if (result.Types.ContainsKey(entry.Key)) {
                    result.Types[entry.Key] = entry.Value;
                } else {
                    result.Types.Add(entry.Key, entry.Value);
                }
            }

            foreach (var entry in b.CommonFunctions) {
                if (result.CommonFunctions.ContainsKey(entry.Key)) {
                    result.CommonFunctions[entry.Key] = entry.Value;
                } else {
                    result.CommonFunctions.Add(entry.Key, entry.Value);
                }
            }

            foreach (var entry in b.FieldAccessors) {
                if (result.FieldAccessors.ContainsKey(entry.Key)) {
                    result.FieldAccessors[entry.Key] = entry.Value;
                } else {
                    result.FieldAccessors.Add(entry.Key, entry.Value);
                }
            }

            foreach (var entry in b.FieldMutators) {
                if (result.FieldMutators.ContainsKey(entry.Key)) {
                    result.FieldMutators[entry.Key] = entry.Value;
                } else {
                    result.FieldMutators.Add(entry.Key, entry.Value);
                }
            }

            foreach (var entry in b.Constructors) {
                if (result.Constructors.ContainsKey(entry.Key)) {
                    result.Constructors[entry.Key] = entry.Value;
                } else {
                    result.Constructors.Add(entry.Key, entry.Value);
                }
            }

            foreach(var entry in b.StructInits) {
                if (result.StructInits.ContainsKey(entry.Key)) {
                    result.StructInits[entry.Key] = entry.Value;
                } else {
                    result.StructInits.Add(entry.Key, entry.Value);
                }
            }

            foreach (var entry in b.InterfaceBindings) {
                if (!result.InterfaceBindings.ContainsKey(entry.Key)) {
                    result.InterfaceBindings.Add(entry.Key, new Dictionary<Type, InterfaceBinding>());
                }

                foreach (var sub in entry.Value) {
                    if (result.InterfaceBindings[entry.Key].ContainsKey(sub.Key)) {
                        result.InterfaceBindings[entry.Key][sub.Key] = sub.Value;
                    } else {
                        result.InterfaceBindings[entry.Key].Add(sub.Key, sub.Value);
                    }
                }
            }

            foreach(var entry in b.SubtypingConversions) {
                if (!result.SubtypingConversions.ContainsKey(entry.Key)) {
                    result.SubtypingConversions.Add(entry.Key, new List<ReductionDeclaration>());
                }

                result.SubtypingConversions[entry.Key].AddRange(b.SubtypingConversions[entry.Key]);
            }

            return result;
        }
    }
}
