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

        public CilScope(TypeBuilder scope, IEnumerable<ReductionDeclaration> functions, ITypeLookup typeLookup)
        {
            this.typeLookup = typeLookup;
            functionStubs = functions
                .ToDictionary(fn => fn, fn => scope.DefineMethod(
                    GetNameFor(fn),
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                    typeLookup[fn.Returns.EffectiveType],
                    fn.Takes.Where(t => !t.IsIdentifier).Select(t =>
                        t.Parameter.Returns.ImplementationType == KindOfType.SingleValue ?
                        typeLookup[((SingleValueType)t.Parameter.Returns).ValueType] :
                        typeLookup[t.Parameter.Returns]).ToArray()));

            var specializations = functions.Where(fn => fn.Takes.Any(pp => !pp.IsIdentifier && pp.Parameter.Returns.ImplementationType == KindOfType.SingleValue)).ToList();
            foreach (var fn in functionStubs.Keys.ToList())
            {
                var fnSpecializations = specializations.Where(fnsp => fnsp.IsSpecializationOf(fn)).ToList();
                this.specializations.Add(fn, fnSpecializations);
            }
        }

        public void Compile(IFunctionCompiler compiler)
        {
            foreach (var kvp in functionStubs)
            {
                compiler.BuildFunctionImplementation(kvp.Key, kvp.Value, specializations[kvp.Key], scope, this, typeLookup);
            }
        }

        public static string GetNameFor(ReductionDeclaration rule)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var entry in rule.Takes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(" ");
                }

                if (entry.IsIdentifier)
                {
                    sb.Append(entry.Identifier.Value);
                }
                else
                {
                    sb.AppendFormat("({0})", GetNameFor(entry.Parameter));
                }
            }

            // TODO: append result name since functions can vary by return type only.

            return sb.ToString();
        }


        public static string GetNameFor(ParameterDeclaration rule)
        {
            // TODO: append result name since parameters can vary by return type only?
            string result = string.Join(" ", rule.Takes.Select(id => id.Value));
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
