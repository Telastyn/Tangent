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
    public class DelegatingTypeLookup : ITypeLookup, IDisposable
    {
        private readonly ITypeCompiler typeCompiler;
        private readonly IEnumerable<TypeDeclaration> declaredTypes;
        private readonly Dictionary<TangentType, Type> lookup = new Dictionary<TangentType, Type>();
        private readonly Dictionary<string, TypeBuilder> partialTypes = new Dictionary<string, TypeBuilder>();
        private readonly AppDomain compilationDomain;

        public DelegatingTypeLookup(ITypeCompiler typeCompiler, IEnumerable<TypeDeclaration> declaredTypes, AppDomain compilationDomain)
        {
            this.typeCompiler = typeCompiler;
            this.declaredTypes = declaredTypes;
            this.compilationDomain = compilationDomain;
            this.compilationDomain.TypeResolve += OnTypeResolution;
        }

        private Assembly OnTypeResolution(object sender, ResolveEventArgs args)
        {
            if (partialTypes.ContainsKey(args.Name)) {
                var builder = partialTypes[args.Name];
                var mapping = lookup.FirstOrDefault(kvp => kvp.Value == builder);
                lookup[mapping.Key] = builder.CreateType();
                partialTypes.Remove(args.Name);

                return lookup[mapping.Key].Assembly;
            } else {
                return null;
            }
        }

        public Type this[TangentType t]
        {
            get
            {
                if (!lookup.ContainsKey(t)) {
                    PopulateLookupWith(t);
                }

                return lookup[t];
            }
        }


        public void BakeTypes()
        {
            var candidates = lookup.Where(kvp => kvp.Value is TypeBuilder).ToList();
            int ct = candidates.Count;
            while (candidates.Count != 0) {
                ct = candidates.Count;
                foreach (var entry in candidates) {
                    lookup[entry.Key] = (entry.Value as TypeBuilder).CreateType();
                }

                candidates = lookup.Where(kvp => kvp.Value is TypeBuilder).ToList();
                if (ct == candidates.Count) {
                    throw new ApplicationException(string.Format("Type are stuck as type builders: {0}", string.Join(", ", candidates.Select(c => c.Value.Name))));
                }
            }
        }

        public void AddGenericFunctionParameterMapping(ParameterDeclaration generic, GenericTypeParameterBuilder dotnetType)
        {
            var reference = GenericArgumentReferenceType.For(generic);
            var inference = GenericInferencePlaceholder.For(generic);
            if (!lookup.ContainsKey(reference)) {
                lookup.Add(reference, dotnetType);
            }

            if (!lookup.ContainsKey(inference)) {
                lookup.Add(inference, dotnetType);
            }
        }

        private void AddLookupEntry(TangentType tt, Type t)
        {
            if (lookup.ContainsKey(tt)) {
                if (partialTypes.ContainsKey(lookup[tt].Name)) {
                    partialTypes.Remove(lookup[tt].Name);
                }

                lookup[tt] = t;
            } else {
                lookup.Add(tt, t);
            }

            if (t is TypeBuilder) {
                partialTypes.Add(t.Name, t as TypeBuilder);
            }
        }

        private void PopulateLookupWith(TangentType t)
        {
            switch (t.ImplementationType) {
                case KindOfType.Enum:
                case KindOfType.Product:
                case KindOfType.Sum:
                case KindOfType.BoundGeneric:
                case KindOfType.Delegate:
                case KindOfType.TypeClass:
                    // This should already be declared in our types.
                    var result = declaredTypes.FirstOrDefault(td => td.Returns == t);
                    if (result == null) {
                        result = new TypeDeclaration((PhrasePart)null, t);
                    }

                    var type = typeCompiler.Compile(result, (placeholderTarget, placeholder) => { if (!lookup.ContainsKey(placeholderTarget)) { lookup.Add(placeholderTarget, placeholder); } }, (tt, create) => { if (create) { return this[tt]; } else { if (lookup.ContainsKey(tt)) { return lookup[tt]; } else { return null; } } });
                    AddLookupEntry(result.Returns, type);

                    return;

                case KindOfType.BoundGenericProduct:
                    var binding = t as BoundGenericProductType;
                    var genericType = lookup[binding.GenericProductType];
                    var arguments = binding.TypeArguments.Select(a => lookup[a]);
                    lookup.Add(t, genericType.MakeGenericType(arguments.ToArray()));
                    return;

                case KindOfType.SingleValue:
                    throw new NotImplementedException("Something is asking for the type of a SingleValueType. We should never get here.");

                case KindOfType.Builtin:
                    if (t == TangentType.Void) {
                        lookup.Add(TangentType.Void, typeof(void));
                    } else if (t == TangentType.String) {
                        lookup.Add(TangentType.String, typeof(string));
                    } else if (t == TangentType.Int) {
                        lookup.Add(TangentType.Int, typeof(int));
                    } else if (t == TangentType.Double) {
                        lookup.Add(TangentType.Double, typeof(double));
                    } else if (t == TangentType.Bool) {
                        lookup.Add(TangentType.Bool, typeof(bool));
                    } else if (t == TangentType.Any) {
                        lookup.Add(TangentType.Any, typeof(object));
                    } else {
                        throw new NotImplementedException();
                    }

                    return;

                case KindOfType.Kind:
                    // For now, all we know is `kind of any`
                    if (t == TangentType.Any.Kind) {
                        lookup.Add(TangentType.Any.Kind, typeof(object));
                    } else {
                        throw new NotImplementedException();
                    }

                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            compilationDomain.TypeResolve -= OnTypeResolution;
        }
    }
}
