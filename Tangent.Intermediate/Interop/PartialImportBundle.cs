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

        public static implicit operator ImportBundle(PartialImportBundle partial)
        {
            return new ImportBundle(partial.Types.Values, partial.CommonFunctions.Values, partial.InterfaceBindings.Values.SelectMany(x => x.Values));
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

            foreach (var entry in a.InterfaceBindings) {
                result.InterfaceBindings.Add(entry.Key, new Dictionary<Type, InterfaceBinding>());
                foreach (var sub in entry.Value) {
                    result.InterfaceBindings[entry.Key].Add(sub.Key, sub.Value);
                }
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

            return result;
        }
    }
}
