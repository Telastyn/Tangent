using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tangent.Tokenization;

namespace Tangent.Intermediate.Interop
{
    public class DotNetType : TangentType
    {
        public readonly Type MappedType;

        private DotNetType(Type target) : base(KindOfType.Builtin)
        {
            MappedType = target;
        }

        private static readonly ConcurrentDictionary<Type, TangentType> cache = new ConcurrentDictionary<Type, TangentType>();
        private static readonly ConcurrentDictionary<Type, TypeDeclaration> declarationCache = new ConcurrentDictionary<Type, TypeDeclaration>();
        public static TangentType For(Type t)
        {
            // TODO: handle bool as an enum type.
            if (t == typeof(bool)) {
                // Doing things this way to avoid initialization loops.
                BoolEnumAdapterType.Common = BoolEnumAdapterType.Common ?? new BoolEnumAdapterType();
                return BoolEnumAdapterType.Common;
            }

            if (t.IsEnum) {
                return cache.GetOrAdd(t, x => DotNetEnumType.For(t));
            }

            // TODO: interfaces.
            if (t.IsInterface) {
                return null;
            }

            return cache.GetOrAdd(t, x => new DotNetType(t));
        }

        public static TypeDeclaration TypeDeclarationFor(Type t)
        {
            // TODO: Action and Func should be delegate types.
            return declarationCache.GetOrAdd(t, x => {
                var tt = DotNetType.For(t);
                if (tt == null) { return null; }
                var phrase = Tokenize.ProgramFile(".NET " + Regex.Replace(t.FullName ?? t.Name, "`[0-9]", ""), "").Select(token => new PhrasePart(token.Value)).ToList();
                if (t.IsGenericTypeDefinition) {
                    // TODO: constraints.
                    var genericParameters = t.GetGenericArguments().Select(ga => new ParameterDeclaration(ga.Name, TangentType.Any.Kind)).ToList();
                    phrase.AddRange(FormatGenericParameters(genericParameters));
                }

                return new TypeDeclaration(phrase, tt);
            });
        }


        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType RebindInferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            return this;
        }

        public override string ToString()
        {
            return ".NET " + Regex.Replace(MappedType.FullName ?? MappedType.Name, "`[0-9]", "") + GenericSignature;
        }

        private static IEnumerable<PhrasePart> FormatGenericParameters(List<ParameterDeclaration> parameters)
        {
            yield return new PhrasePart("<");
            for (int x = 0; x < parameters.Count; ++x) {
                yield return new PhrasePart(parameters[x]);
                if (x != parameters.Count - 1) {
                    yield return new PhrasePart(",");
                }
            }

            yield return new PhrasePart(">");
        }

        private string GenericSignature
        {
            get
            {
                if (MappedType.IsGenericTypeDefinition) {
                    return "<" + string.Join("", Enumerable.Repeat(",", MappedType.GetGenericArguments().Length - 1)) + ">";
                }

                return "";
            }
        }
    }
}
