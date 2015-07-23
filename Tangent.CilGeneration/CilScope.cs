using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class CilScope : IFunctionLookup
    {
        private readonly Dictionary<ReductionDeclaration, MethodBuilder> functionStubs;
        private readonly Dictionary<ReductionDeclaration, IEnumerable<ReductionDeclaration>> specializations = new Dictionary<ReductionDeclaration, IEnumerable<ReductionDeclaration>>();
        private readonly ITypeLookup typeLookup;
        private readonly TypeBuilder scope;

        public CilScope(TypeBuilder scope, IEnumerable<ReductionDeclaration> functions, IEnumerable<ReductionDeclaration> builtIns, ITypeLookup typeLookup)
        {
            this.typeLookup = typeLookup;

            functionStubs = functions
                .ToDictionary(fn => fn, fn =>
                {
                    var dotnetFn = scope.DefineMethod(
                        GetNameFor(fn),
                        System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static);

                    if (fn.GenericParameters.Any()) {
                        var dotNetGenerics = dotnetFn.DefineGenericParameters(fn.GenericParameters.Select(pd => string.Join(" ", pd.Takes)).ToArray());

                        // TODO: constraints

                        foreach (var entry in fn.GenericParameters.Zip(dotNetGenerics, (pd, g) => Tuple.Create(pd, g))) {
                            typeLookup.AddGenericFunctionParameterMapping(entry.Item1, entry.Item2);
                        }
                    }

                    dotnetFn.SetReturnType(typeLookup[fn.Returns.EffectiveType]);
                    dotnetFn.SetParameters(
                        fn.Takes.Where(t => !t.IsIdentifier).Select(t =>
                            t.Parameter.Returns.ImplementationType == KindOfType.SingleValue ?
                            typeLookup[((SingleValueType)t.Parameter.Returns).ValueType] :
                            typeLookup[t.Parameter.Returns]).ToArray());

                    return dotnetFn;
                });

            foreach (var fn in functionStubs.Keys.ToList()) {
                var fnSpecializations = functions.Concat(builtIns).Where(fnsp => fnsp.IsSpecializationOf(fn)).ToList();
                this.specializations.Add(fn, fnSpecializations);
            }

            this.scope = scope;
        }

        public void Compile(IFunctionCompiler compiler)
        {
            foreach (var kvp in functionStubs) {
                compiler.BuildFunctionImplementation(kvp.Key, kvp.Value, specializations[kvp.Key], scope, this, typeLookup);
            }
        }

        public string GetNameFor(ReductionDeclaration rule)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var entry in rule.Takes) {
                if (first) {
                    first = false;
                } else {
                    sb.Append(" ");
                }

                if (entry.IsIdentifier) {
                    sb.Append(entry.Identifier.Value);
                } else {
                    sb.AppendFormat("({0})", GetNameFor(entry.Parameter));
                }
            }

            sb.AppendFormat(" => {0}", typeLookup[rule.Returns.EffectiveType].Name);

            sb.Insert(0, string.Join(",", rule.GenericParameters.Select(pd => string.Format("<{0}>", string.Join(" ", pd.Takes)))));
            return sb.ToString();
        }


        public string GetNameFor(ParameterDeclaration rule)
        {
            return GetNameFor(rule, this.typeLookup);
        }

        public static string GetNameFor(ParameterDeclaration rule, ITypeLookup typeLookup) { return GetNameFor(rule, tt => typeLookup[tt]); }

        public static string GetNameFor(ParameterDeclaration rule, Func<TangentType, Type> typeLookup)
        {
            string paramTypeName;
            if (rule.Returns.ImplementationType == KindOfType.SingleValue) {
                var svt = ((SingleValueType)rule.Returns);
                paramTypeName = typeLookup(svt.ValueType).Name + "." + svt.Value.Value;
            } else {
                if (!rule.Returns.ContainedGenericReferences(GenericTie.Inference).Any()) {
                    paramTypeName = typeLookup(rule.Returns).Name;
                } else {
                    paramTypeName = "<inference>";
                }
            }

            string result = string.Join(" ", rule.Takes.Select(id => id.Value)) + ": " + paramTypeName;
            return result;
        }

        public MethodInfo this[ReductionDeclaration fn]
        {
            get
            {
                MethodBuilder result = null;
                functionStubs.TryGetValue(fn, out result);
                return result;
            }
        }
    }
}
