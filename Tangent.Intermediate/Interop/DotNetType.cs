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
    public class DotNetType : TangentType, HasGenericParameters
    {
        public readonly Type MappedType;

        private DotNetType(Type target) : base(KindOfType.Builtin)
        {
            MappedType = target;
        }

        private static readonly ConcurrentDictionary<Type, TangentType> cache = new ConcurrentDictionary<Type, TangentType>();
        private static readonly ConcurrentDictionary<Type, TypeDeclaration> declarationCache = new ConcurrentDictionary<Type, TypeDeclaration>();
        private static readonly ConcurrentDictionary<Type, ParameterDeclaration> genericCache = new ConcurrentDictionary<Type, ParameterDeclaration>();
        public static TangentType For(Type t)
        {
            if (t == typeof(bool)) {
                // Doing things this way to avoid initialization loops.
                BoolEnumAdapterType.Common = BoolEnumAdapterType.Common ?? new BoolEnumAdapterType();
                return BoolEnumAdapterType.Common;
            }

            if (t.IsEnum) {
                return cache.GetOrAdd(t, x => DotNetEnumType.For(t));
            }

            if (t.IsConstructedGenericType) {
                return cache.GetOrAdd(t, x => BuildConstructedGenericType(t));
            }

            if (t.IsGenericParameter) {
                // TODO: constraints.
                return cache.GetOrAdd(t, x => GenericArgumentReferenceType.For(genericCache.GetOrAdd(t, y => new ParameterDeclaration(t.Name, TangentType.Any.Kind))));
            }

            return cache.GetOrAdd(t, x => new DotNetType(t));
        }

        private static TangentType BuildConstructedGenericType(Type t)
        {
            var generic = t.GetGenericTypeDefinition();
            var tangentGeneric = For(generic);
            if (tangentGeneric == null) { return null; }
            var hasGenerics = tangentGeneric as HasGenericParameters;
            if (hasGenerics == null) { throw new ApplicationException("Generic type yielded something without generic parameters?"); }

            var args = t.GetGenericArguments().Select(ga => For(ga)).ToList();
            if (args.Any(a => a == null)) { return null; }

            return BoundGenericType.For(hasGenerics, args);
        }

        public static TypeDeclaration TypeDeclarationFor(Type t)
        {
            // TODO: Action and Func should be delegate types.
            return declarationCache.GetOrAdd(t, x => {
                var tt = DotNetType.For(t);
                if (tt == null) { return null; }
                var phrase = Tokenize.ProgramFile(".NET " + Regex.Replace(t.FullName ?? t.Name, "`[0-9]+", ""), "").Select(token => new PhrasePart(token.Value)).ToList();
                if (t.IsGenericTypeDefinition) {
                    // TODO: constraints.
                    var genericParameters = t.GetGenericArguments().Select(ga => genericCache.GetOrAdd(ga, y => new ParameterDeclaration(ga.Name, TangentType.Any.Kind))).ToList();
                    phrase.AddRange(FormatGenericParameters(genericParameters));
                }

                return new TypeDeclaration(phrase, tt);
            });
        }


        public override bool CompatibilityMatches(TangentType other, Dictionary<ParameterDeclaration, TangentType> necessaryTypeInferences)
        {
            return this == other;
        }

        public override TangentType ResolveGenericReferences(Func<ParameterDeclaration, TangentType> mapping)
        {
            if (this.GenericParameters.Any()) {
                return BoundGenericType.For(this, this.GenericParameters.Select(pd => mapping(pd)));
            }

            return this;
        }

        public override string ToString()
        {
            return ".NET " + Regex.Replace(MappedType.FullName ?? MappedType.Name, "`[0-9]+", "") + GenericSignature;
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

        public IEnumerable<ParameterDeclaration> GenericParameters
        {
            get
            {
                if (!MappedType.IsGenericTypeDefinition) {
                    return Enumerable.Empty<ParameterDeclaration>();
                }

                return TypeDeclarationFor(MappedType).Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
            }
        }

        protected internal override IEnumerable<ParameterDeclaration> ContainedGenericReferences(HashSet<TangentType> alreadyProcessed)
        {
            if (alreadyProcessed.Contains(this)) {
                return Enumerable.Empty<ParameterDeclaration>();
            }

            alreadyProcessed.Add(this);
            return GenericParameters;
        }
    }
}
