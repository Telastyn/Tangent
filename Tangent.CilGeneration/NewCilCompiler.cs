using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Intermediate.Interop;
using Tangent.Runtime;

namespace Tangent.CilGeneration
{
    // RMS: making this big and monolithic since splitting it up made it complex, but still pretty highly coupled.
    public class NewCilCompiler : IDisposable
    {
        private readonly TangentProgram program;

        private readonly Dictionary<ReductionDeclaration, MethodInfo> functionLookup = new Dictionary<ReductionDeclaration, MethodInfo>();
        private readonly Dictionary<TangentType, Type> typeLookup = new Dictionary<TangentType, Type>();
        private readonly Dictionary<Field, FieldInfo> fieldLookup = new Dictionary<Field, FieldInfo>();
        private readonly Dictionary<ParameterDeclaration, FieldInfo> ctorParamLookup = new Dictionary<ParameterDeclaration, FieldInfo>();
        private readonly Dictionary<TangentType, ConstructorInfo> productCtorLookup = new Dictionary<TangentType, ConstructorInfo>();
        private readonly Dictionary<TangentType, Dictionary<Type, ConstructorInfo>> variantCtorLookup = new Dictionary<TangentType, Dictionary<Type, ConstructorInfo>>();
        private readonly Dictionary<TangentType, FieldInfo> variantValueLookup = new Dictionary<TangentType, FieldInfo>();
        private readonly Dictionary<TangentType, FieldInfo> variantModeLookup = new Dictionary<TangentType, FieldInfo>();


        private Dictionary<string, ISymbolDocumentWriter> debuggingSymbolLookup;
        private ModuleBuilder targetModule;
        private TypeBuilder rootType;
        private readonly AppDomain compilationDomain;
        // TODO: options?

        private int anonymousTypeIndex = 0;
        private int closureCounter = 0;

        private NewCilCompiler(TangentProgram program)
        {
            this.program = program;
            this.compilationDomain = AppDomain.CurrentDomain;
            // TODO: is this still necessary?
            this.compilationDomain.TypeResolve += OnTypeResolution;
        }

        public static void Compile(TangentProgram program, string targetPath)
        {
            using (var compiler = new NewCilCompiler(program)) {
                compiler.Compile(targetPath);
            }
        }

        private void Compile(string targetPath)
        {
            var entrypoint = program.Functions.FirstOrDefault(
                fn => fn.Takes.Count == 1 &&
                    fn.Takes.First().IsIdentifier &&
                    fn.Takes.First().Identifier.Value == "entrypoint" &&
                    fn.Returns.EffectiveType == TangentType.Void);

            if (entrypoint == null) {
                throw new InvalidOperationException("Specified Tangent program has no entrypoint.");
            }

            // TODO: replace with something more exensible.
            foreach (var builtinFn in Tangent.Intermediate.Interop.BuiltinFunctions.All.Select(fn => new { Decl = fn, MI = Tangent.Intermediate.Interop.BuiltinFunctions.DotNetFunctionForBuiltin(fn) }).Where(fn => fn.MI != null)) {
                functionLookup.Add(builtinFn.Decl, builtinFn.MI);
            }

            // TODO: replace with something more exensible.
            typeLookup.Add(TangentType.Void, typeof(void));
            typeLookup.Add(TangentType.String, typeof(string));
            typeLookup.Add(TangentType.Int, typeof(int));
            typeLookup.Add(TangentType.Double, typeof(double));
            typeLookup.Add(TangentType.Bool, typeof(bool));
            typeLookup.Add(TangentType.Any, typeof(object));
            typeLookup.Add(TangentType.Any.Kind, typeof(object));

            string filename = Path.GetFileNameWithoutExtension(targetPath);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName(filename), AssemblyBuilderAccess.Save);
            targetModule = assemblyBuilder.DefineDynamicModule(filename + ".exe", Path.GetFileName(targetPath) + ".exe", true);

            debuggingSymbolLookup = program.InputLabels.ToDictionary(l => l, l => targetModule.DefineDocument(l, Guid.Empty, Guid.Empty, Guid.Empty));
            rootType = targetModule.DefineType("_");
            AddGlobals(rootType, program.Fields);

            Compile(entrypoint);

            rootType.CreateType();
            foreach (var typeBuilder in typeLookup.Values.Where(t => t is TypeBuilder).Cast<TypeBuilder>()) {
                typeBuilder.CreateType();
            }

            if (entrypoint != null) {
                var entrypointMethod = functionLookup[entrypoint];
                assemblyBuilder.SetEntryPoint(entrypointMethod);
            }

            assemblyBuilder.Save(targetPath + ".exe");
        }

        private Type Compile(TangentType target)
        {
            if (typeLookup.ContainsKey(target)) {
                return typeLookup[target];
            }

            TypeDeclaration typeDecl;
            switch (target.ImplementationType) {
                case KindOfType.SingleValue:
                    return Compile(((SingleValueType)target).ValueType);
                case KindOfType.Enum:
                    typeDecl = program.TypeDeclarations.First(td => td.Returns == target);
                    return BuildEnum(typeDecl);
                case KindOfType.Product:
                    typeDecl = program.TypeDeclarations.FirstOrDefault(td => td.Returns == target);
                    if (typeDecl == null) {
                        // Some anonymous product type in a sum type.
                        typeDecl = new TypeDeclaration((PhrasePart)null, target);
                    }

                    return BuildClass(typeDecl);
                case KindOfType.TypeClass:
                    typeDecl = program.TypeDeclarations.First(td => td.Returns == target);
                    return BuildVariant(typeDecl);
                case KindOfType.BoundGeneric:
                    return InstantiateGeneric(target as BoundGenericType);
                case KindOfType.Delegate:
                    return BuildDelegateType(target as DelegateType);
                case KindOfType.Builtin:
                    var dnt = target as DotNetType;
                    if (dnt != null) {
                        return dnt.MappedType;
                    }

                    var dnet = target as DotNetEnumType;
                    if (dnet != null) {
                        return dnet.DotNetType;
                    }

                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private MethodInfo Compile(ReductionDeclaration fn)
        {
            if (functionLookup.ContainsKey(fn)) {
                return functionLookup[fn];
            }

            var stub = rootType.DefineMethod(GetNameFor(fn), MethodAttributes.Public | MethodAttributes.Static);

            if (fn.GenericParameters.Any()) {
                var dotNetGenerics = stub.DefineGenericParameters(fn.GenericParameters.Select(pd => string.Join(" ", pd.Takes)).ToArray());

                // TODO: constraints
                foreach (var entry in fn.GenericParameters.Zip(dotNetGenerics, (pd, g) => Tuple.Create(pd, g))) {
                    var genRef = GenericArgumentReferenceType.For(entry.Item1);
                    if (!typeLookup.ContainsKey(genRef)) { typeLookup.Add(genRef, entry.Item2); }
                }
            }

            stub.SetReturnType(Compile(fn.Returns.EffectiveType));
            stub.SetParameters(
                fn.Takes.Where(t => !t.IsIdentifier && t.Parameter.RequiredArgumentType.ImplementationType != KindOfType.Kind).Select(t =>
                    t.Parameter.RequiredArgumentType.ImplementationType == KindOfType.SingleValue ?
                    Compile(((SingleValueType)t.Parameter.RequiredArgumentType).ValueType) :
                    Compile(t.Parameter.RequiredArgumentType)).ToArray());

            functionLookup.Add(fn, stub);

            BuildImplementation(fn, stub);

            return stub;
        }

        private Type BuildEnum(TypeDeclaration decl)
        {
            var typeName = GetNameFor(decl);
            var enumBuilder = targetModule.DefineEnum(typeName, System.Reflection.TypeAttributes.Public, typeof(int));
            typeLookup.Add(decl.Returns, enumBuilder);
            int x = 1;
            foreach (var value in (decl.Returns as EnumType).Values) {
                enumBuilder.DefineLiteral(value.Value, x++);
            }

            return enumBuilder.CreateType();
        }

        private Type BuildClass(TypeDeclaration decl)
        {
            var typeName = GetNameFor(decl);
            if (typeName.Contains(".")) {
                throw new NotImplementedException("TODO: fix dots in typenames causing CIL to namespace things.");
            }

            var productType = (ProductType)decl.Returns;

            var classBuilder = targetModule.DefineType(typeName, System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public);
            typeLookup.Add(decl.Returns, classBuilder);

            if (decl.IsGeneric) {
                var genericRefs = decl.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter);
                var genericBuilders = classBuilder.DefineGenericParameters(genericRefs.Select(pd => GetNameFor(pd)).ToArray());
                foreach (var entry in genericRefs.Zip(genericBuilders, (pd, gb) => Tuple.Create(pd, gb))) {
                    typeLookup.Add(GenericArgumentReferenceType.For(entry.Item1), entry.Item2);
                }
            }

            var tangentCtorParams = productType.DataConstructorParts.Where(pp => !pp.IsIdentifier && pp.Parameter.RequiredArgumentType.ImplementationType != KindOfType.Kind).ToList();
            var dotnetCtorParamTypes = tangentCtorParams.Select(pp => Compile(pp.Parameter.RequiredArgumentType)).ToList();
            var ctor = classBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, dotnetCtorParamTypes.ToArray());
            productCtorLookup.Add(productType, ctor);
            var gen = ctor.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, classBuilder.BaseType.GetConstructor(new Type[0]));

            for (int ix = 0; ix < dotnetCtorParamTypes.Count; ++ix) {
                var field = classBuilder.DefineField(GetNameFor(tangentCtorParams[ix].Parameter), dotnetCtorParamTypes[ix], System.Reflection.FieldAttributes.Public | System.Reflection.FieldAttributes.InitOnly);
                ctorParamLookup.Add(tangentCtorParams[ix].Parameter, field);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg, ix + 1);
                gen.Emit(OpCodes.Stfld, field);
            }

            foreach (var entry in productType.Fields) {
                var field = classBuilder.DefineField(GetNameFor(entry.Declaration), Compile(entry.Declaration.Returns), System.Reflection.FieldAttributes.Public);
                fieldLookup.Add(entry, field);
                gen.Emit(OpCodes.Ldarg_0);
                var thisAccessor = new Dictionary<ParameterDeclaration, PropertyCodes>() { { entry.Declaration.Takes.First(pp => !pp.IsIdentifier).Parameter, new PropertyCodes(g => g.Emit(OpCodes.Ldarg_0), null) } };
                // TODO: make sure closures work here.
                AddExpression(entry.Initializer, gen, thisAccessor, null, false);
                gen.Emit(OpCodes.Stfld, field);
            }

            gen.Emit(OpCodes.Ret);
            return classBuilder;
        }

        private void AddGlobals(TypeBuilder rootType, IEnumerable<Field> fields)
        {
            var staticCtor = rootType.DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, new Type[0]);
            var gen = staticCtor.GetILGenerator();

            foreach (var entry in fields) {
                var field = rootType.DefineField(GetNameFor(entry.Declaration), Compile(entry.Declaration.Returns), FieldAttributes.Static | FieldAttributes.Public);
                fieldLookup.Add(entry, field);
                AddExpression(entry.Initializer, gen, new Dictionary<ParameterDeclaration, PropertyCodes>(), null, false);
                gen.Emit(OpCodes.Stsfld, field);
            }

            gen.Emit(OpCodes.Ret);
        }

        private Type BuildVariant(TypeDeclaration decl)
        {
            var typeName = GetNameFor(decl);
            var variantParts = new List<TangentType>();
            switch (decl.Returns.ImplementationType) {
                case KindOfType.TypeClass:
                    variantParts.AddRange(((TypeClass)decl.Returns).Implementations);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (!variantParts.Any()) {
                throw new NotImplementedException("Sorry, interfaces without implementations are current unsupported.");
            }

            var classBuilder = targetModule.DefineType(typeName);
            typeLookup.Add(decl.Returns, classBuilder);
            variantCtorLookup.Add(decl.Returns, new Dictionary<Type, ConstructorInfo>());

            if (decl.IsGeneric) {
                var genericParamDefs = decl.Takes.Where(pp => !pp.IsIdentifier).ToList();
                var genericBuilders = classBuilder.DefineGenericParameters(genericParamDefs.Select(pp => GetNameFor(pp.Parameter)).ToArray());
                // TODO: constraints. Since `kind of any` is the only legal kind, skip for now.
                foreach (var entry in genericParamDefs.Select(gpd => GenericArgumentReferenceType.For(gpd.Parameter)).Zip(genericBuilders, (a, b) => Tuple.Create(a, b))) {
                    typeLookup.Add(entry.Item1, entry.Item2);
                }
            }

            // And generic references should now resolve properly. I think.
            var variantTypes = variantParts.Select(tt => Compile(tt)).OrderBy(t => t.Name).ToArray();
            var variantContainer = typeof(Variant<,>).Module.GetTypes().FirstOrDefault(t => t.Name == "Variant`" + variantTypes.Length);
            if (variantContainer == null) {
                throw new ApplicationException("Error finding runtime variant type of size " + variantTypes.Length);
            }

            var parent = variantContainer.MakeGenericType(variantTypes);
            variantValueLookup.Add(decl.Returns, TypeBuilder.GetField(parent, variantContainer.GetField("Value")));
            variantModeLookup.Add(decl.Returns, TypeBuilder.GetField(parent, variantContainer.GetField("Mode")));
            classBuilder.SetParent(parent);
            int ix = 0;
            foreach (var variantType in variantTypes) {
                var ctor = classBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, new[] { variantType });
                variantCtorLookup[decl.Returns].Add(variantType, ctor);
                var gen = ctor.GetILGenerator();

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                //if (parent.GetType().Name == "TypeBuilderInstantiation") {
                // see https://msdn.microsoft.com/en-us/library/system.reflection.emit.generictypeparameterbuilder(v=vs.110).aspx
                // Generics are weird.
                var genericVariantCtor = variantContainer.GetConstructor(new[] { variantContainer.GetGenericArguments()[ix] });
                var baseCtor = TypeBuilder.GetConstructor(parent, genericVariantCtor);
                gen.Emit(OpCodes.Call, baseCtor);
                //} else {
                //    gen.Emit(OpCodes.Call, parent.GetConstructor(new[] { variantType }));
                //}

                gen.Emit(OpCodes.Ret);
                ix++;
            }

            return classBuilder;
        }

        private Type InstantiateGeneric(BoundGenericType boundGeneric)
        {
            Type genericType = Compile(boundGeneric.GenericType);
            var arguments = boundGeneric.TypeArguments.Select(tt => Compile(tt)).ToArray();
            var instance = genericType.MakeGenericType(arguments);
            if (boundGeneric.GenericType.ImplementationType == KindOfType.Product) {
                var genericCtor = productCtorLookup[boundGeneric.GenericType];
                var instanceCtor = TypeBuilder.GetConstructor(instance, genericCtor);
                productCtorLookup.Add(boundGeneric, instanceCtor);
            }

            typeLookup.Add(boundGeneric, instance);
            return instance;
        }

        private Type BuildDelegateType(DelegateType tangentDelegateType)
        {
            Type delegateGenericType;
            Type[] genericArgs;
            if (tangentDelegateType.Returns == TangentType.Void) {
                if (tangentDelegateType.Takes.Count == 0) {
                    return typeof(Action);
                } else {
                    delegateGenericType = typeof(Action).Assembly.GetTypes().First(t => t.Name.StartsWith("Action") && t.IsGenericTypeDefinition && t.GetGenericArguments().Count() == tangentDelegateType.Takes.Count);
                    genericArgs = tangentDelegateType.Takes.Select(tt => Compile(tt)).ToArray();
                }
            } else {
                delegateGenericType = typeof(Func<int>).Assembly.GetTypes().First(t => t.Name.StartsWith("Func") && t.IsGenericTypeDefinition && t.GetGenericArguments().Count() == tangentDelegateType.Takes.Count + 1);
                genericArgs = tangentDelegateType.Takes.Concat(new[] { tangentDelegateType.Returns }).Select(tt => Compile(tt)).ToArray();
            }

            var concreteFuncType = delegateGenericType.MakeGenericType(genericArgs);
            return concreteFuncType;
        }

        private Type GenericDelegateTypeFor(DelegateType tangentDelegateType)
        {
            if (tangentDelegateType.Returns == TangentType.Void) {
                if (tangentDelegateType.Takes.Count == 0) {
                    return typeof(Action);
                } else {
                    return typeof(Action).Assembly.GetTypes().First(t => t.Name.StartsWith("Action") && t.IsGenericTypeDefinition && t.GetGenericArguments().Count() == tangentDelegateType.Takes.Count);
                }
            } else {
                return typeof(Func<int>).Assembly.GetTypes().First(t => t.Name.StartsWith("Func") && t.IsGenericTypeDefinition && t.GetGenericArguments().Count() == tangentDelegateType.Takes.Count + 1);
            }
        }

        private void BuildImplementation(ReductionDeclaration fn, MethodBuilder builder, TypeBuilder parentScope = null)
        {
            var specializations = program.Functions.Where(other => other.IsSpecializationOf(fn)).ToList();
            foreach (var specialization in specializations) {
                // Force specializations to be built so that any generic types exist in our lookup.
                Compile(specialization);
            }

            var gen = builder.GetILGenerator();
            Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes = BuildNormalAccesses(gen, fn);

            AddDispatchCode(gen, fn, specializations, parameterCodes);

            ClosureInfo closureScope = null;
            if (fn.RequiresClosure) {
                parameterCodes = BuildClosureStyleAccesses(gen, fn, parentScope ?? rootType, out closureScope);
            } else {
                foreach (var entry in BuildLocalAccesses(gen, fn)) {
                    parameterCodes.Add(entry.Key, entry.Value);
                }
            }

            var doc = fn.Returns as DirectOpCode;
            if (doc != null) {
                AddExpression(new FunctionInvocationExpression(fn, fn.Takes.Where(pp => !pp.IsIdentifier).Select(pp => new ParameterAccessExpression(pp.Parameter, null)), Enumerable.Empty<TangentType>(), null), gen, parameterCodes, closureScope, true);
            } else {
                AddFunctionCode(gen, fn.Returns.Implementation, parameterCodes, closureScope);
            }

            if (!(fn.Returns is InterfaceFunction)) {
                gen.Emit(OpCodes.Ret);
            }

            if (closureScope != null) {
                closureScope.ClosureType.CreateType();
            }
        }

        private Dictionary<ParameterDeclaration, PropertyCodes> BuildClosureStyleAccesses(ILGenerator gen, ReductionDeclaration fn, TypeBuilder parentScope, out ClosureInfo closureInfo)
        {
            if (parentScope != rootType) {
                throw new NotImplementedException("TODO: implement nested closures.");
            }

            var possiblyGenericClosureType = (parentScope ?? rootType).DefineNestedType("closure" + closureCounter++, TypeAttributes.Sealed | TypeAttributes.NestedPublic);
            var closureGenericParameters = fn.GenericParameters.Any() ? possiblyGenericClosureType.DefineGenericParameters(fn.GenericParameters.Select(gp => GetNameFor(gp)).ToArray()) : new GenericTypeParameterBuilder[] { };
            var closureGenericScope = closureGenericParameters.Zip(fn.GenericParameters, (a, b) => new ClosureGenericMapping(b, typeLookup[GenericArgumentReferenceType.For(b)], a)).ToList();
            var concreteClosureType = fn.GenericParameters.Any() ? possiblyGenericClosureType.MakeGenericType(fn.GenericParameters.Select(gp => Compile(GenericArgumentReferenceType.For(gp))).ToArray()) : possiblyGenericClosureType;

            // Here we need to overwrite the resolution for generic parameters since they're part of the generic instance, not the function.
            foreach (var entry in closureGenericScope) {
                typeLookup[GenericArgumentReferenceType.For(entry.TangentGeneric)] = entry.ClosureGeneric;

                // And we need to get things like Func<T> too.
                var keys = new List<TangentType>();
                foreach (var kvp in typeLookup) {
                    if (kvp.Key != GenericArgumentReferenceType.For(entry.TangentGeneric) && kvp.Key.ContainedGenericReferences().Contains(entry.TangentGeneric)) {
                        keys.Add(kvp.Key);
                    }
                }

                foreach (var key in keys) {
                    typeLookup[key] = Compile(key);
                }
            }


            var scope = gen.DeclareLocal(concreteClosureType);
            scope.SetLocalSymInfo("__closureScope");
            var ctor = possiblyGenericClosureType.DefineDefaultConstructor(MethodAttributes.Public);
            if (fn.GenericParameters.Any()) {
                gen.Emit(OpCodes.Newobj, TypeBuilder.GetConstructor(concreteClosureType, ctor));
            } else {
                gen.Emit(OpCodes.Newobj, ctor);
            }

            gen.Emit(OpCodes.Stloc, scope);

            Dictionary<ParameterDeclaration, PropertyCodes> result = new Dictionary<ParameterDeclaration, PropertyCodes>();
            var closureCodes = new Dictionary<ParameterDeclaration, PropertyCodes>();

            int ix = 0;
            foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier && pp.Parameter.RequiredArgumentType.ImplementationType != KindOfType.Kind).Select(pp => pp.Parameter)) {
                FieldInfo paramField = possiblyGenericClosureType.DefineField(GetNameFor(parameter), Compile(parameter.Returns), FieldAttributes.Public);
                FieldInfo closureParamField = paramField;
                if (fn.GenericParameters.Any()) {
                    paramField = TypeBuilder.GetField(concreteClosureType, paramField);

                }

                gen.Emit(OpCodes.Ldloc, scope);
                gen.Emit(OpCodes.Ldarg, ix);
                gen.Emit(OpCodes.Stfld, paramField);

                result.Add(parameter, new PropertyCodes(
                    g => { g.Emit(OpCodes.Ldloc, scope); g.Emit(OpCodes.Ldfld, paramField); },
                    (g, v) => { g.Emit(OpCodes.Ldloc, scope); v(); g.Emit(OpCodes.Stfld, paramField); }));

                closureCodes.Add(parameter, new PropertyCodes(
                    g => { g.Emit(OpCodes.Ldarg_0); g.Emit(OpCodes.Ldfld, closureParamField); },
                    (g, v) => { g.Emit(OpCodes.Ldarg_0); v(); g.Emit(OpCodes.Stfld, closureParamField); }));

                ix++;
            }

            foreach (var local in fn.Returns.Implementation.Locals) {
                FieldInfo localField = possiblyGenericClosureType.DefineField(GetNameFor(local), Compile(local.Returns), FieldAttributes.Public);
                FieldInfo closureLocalField = localField;
                if (fn.GenericParameters.Any()) {
                    localField = TypeBuilder.GetField(concreteClosureType, localField);
                }

                result.Add(local, new PropertyCodes(
                    g => { g.Emit(OpCodes.Ldloc, scope); g.Emit(OpCodes.Ldfld, localField); },
                    (g, v) => { g.Emit(OpCodes.Ldloc, scope); v(); g.Emit(OpCodes.Stfld, localField); }));

                closureCodes.Add(local, new PropertyCodes(
                    g => { g.Emit(OpCodes.Ldarg_0); g.Emit(OpCodes.Ldfld, closureLocalField); },
                    (g, v) => { g.Emit(OpCodes.Ldarg_0); v(); g.Emit(OpCodes.Stfld, closureLocalField); }));
            }

            foreach (var entry in closureGenericScope) {
                typeLookup[GenericArgumentReferenceType.For(entry.TangentGeneric)] = entry.FunctionGeneric;

                var keys = new List<TangentType>();
                foreach (var kvp in typeLookup) {
                    if (kvp.Key != GenericArgumentReferenceType.For(entry.TangentGeneric) && kvp.Key.ContainedGenericReferences().Contains(entry.TangentGeneric)) {
                        keys.Add(kvp.Key);
                    }
                }

                foreach (var key in keys) {
                    typeLookup[key] = Compile(key);
                }
            }

            closureInfo = new ClosureInfo(possiblyGenericClosureType, closureCodes, g => g.Emit(OpCodes.Ldloc, scope), closureGenericScope, null);
            return result;
        }

        private Dictionary<ParameterDeclaration, PropertyCodes> BuildNormalAccesses(ILGenerator gen, ReductionDeclaration fn)
        {
            var parameterCodes = fn.Takes.Where(pp => !pp.IsIdentifier && pp.Parameter.RequiredArgumentType.ImplementationType != KindOfType.Kind).Select((pp, ix) => new KeyValuePair<ParameterDeclaration, PropertyCodes>(pp.Parameter, new PropertyCodes(g => g.Emit(OpCodes.Ldarg, (Int16)ix), null))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return parameterCodes;
        }

        private Dictionary<ParameterDeclaration, PropertyCodes> BuildLocalAccesses(ILGenerator gen, ReductionDeclaration fn)
        {
            var parameterCodes = new Dictionary<ParameterDeclaration, PropertyCodes>();
            if (fn.Returns.Implementation == null) { return parameterCodes; }

            int localix = 0;
            foreach (var local in fn.Returns.Implementation.Locals) {
                var lb = gen.DeclareLocal(Compile(local.Returns));
                lb.SetLocalSymInfo(GetNameFor(local));
                var closureIx = localix;
                parameterCodes.Add(local, new PropertyCodes(g => g.Emit(OpCodes.Ldloc, closureIx), (g, v) => { v(); g.Emit(OpCodes.Stloc, closureIx); }));
                localix++;
            }

            return parameterCodes;
        }

        private void AddDispatchCode(ILGenerator gen, ReductionDeclaration fn, IEnumerable<ReductionDeclaration> specializations, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes)
        {
            if (!specializations.Any()) {
                return;
            }

            var objGetType = typeof(object).GetMethod("GetType");
            var typeEquality = typeof(Type).GetMethod("op_Equality");
            var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
            var isGenericType = typeof(Type).GetProperty("IsGenericType").GetGetMethod();
            var getGenericTypeDefinition = typeof(Type).GetMethod("GetGenericTypeDefinition");
            var getMethodFromHandle = typeof(MethodBase).GetMethods().Where(mi => mi.Name == "GetMethodFromHandle" && mi.GetParameters().Count() == 1).First();
            var makeGenericMethod = typeof(MethodInfo).GetMethod("MakeGenericMethod");
            var invoke = typeof(MethodInfo).GetMethods().Where(mi => mi.Name == "Invoke" && mi.GetParameters().Count() == 2).First();
            var getGenericArguments = typeof(Type).GetMethod("GetGenericArguments");
            var unboxingNeeded = gen.DeclareLocal(typeof(bool));
            var parameterTypeLocals = new Dictionary<ParameterDeclaration, LocalBuilder>();

            // For now, we can't have nested specializations, so just go in order, doing the checks.
            foreach (var specialization in specializations) {
                Label next = gen.DefineLabel();
                var specializationDetails = specialization.SpecializationAgainst(fn).Specializations;
                var modes = new Dictionary<ParameterDeclaration, Tuple<Type, Type>>();
                var specialCasts = new Dictionary<ParameterDeclaration, Type>();
                foreach (var specializationParam in specializationDetails) {
                    switch (specializationParam.SpecializationType) {
                        case DispatchType.SingleValue:
                            var single = (SingleValueType)specializationParam.SpecificFunctionParameter.Returns;

                            // If the specialization is not met, go to next specialization.
                            parameterCodes[specializationParam.GeneralFunctionParameter].Accessor(gen);
                            if (single.ValueType is BoolEnumAdapterType) {
                                if (single.Value.Value == "true") {
                                    gen.Emit(OpCodes.Brfalse, next);
                                } else {
                                    gen.Emit(OpCodes.Brtrue, next);
                                }
                            } else {
                                gen.Emit(OpCodes.Ldc_I4, single.NumericEquivalent);
                                gen.Emit(OpCodes.Ceq);
                                gen.Emit(OpCodes.Brfalse, next);
                                // Otherwise, proceed to next param.
                            }
                            break;

                        case DispatchType.SumType:
                            var dotNetSum = Compile(specializationParam.GeneralFunctionParameter.Returns);
                            var dotNetTarget = Compile(specializationParam.SpecificFunctionParameter.Returns);
                            var targetMode = GetVariantMode(dotNetSum, dotNetTarget);
                            modes.Add(specializationParam.GeneralFunctionParameter, Tuple.Create(dotNetSum, dotNetTarget));
                            var modeField = variantModeLookup[specializationParam.GeneralFunctionParameter.Returns];
                            parameterCodes[specializationParam.GeneralFunctionParameter].Accessor(gen);
                            gen.Emit(OpCodes.Ldfld, modeField);
                            gen.Emit(OpCodes.Ldc_I4, targetMode);
                            gen.Emit(OpCodes.Ceq);
                            gen.Emit(OpCodes.Brfalse, next);
                            break;

                        case DispatchType.GenericSpecialization:
                            var gart = (specializationParam.GeneralFunctionParameter.RequiredArgumentType as GenericArgumentReferenceType);
                            var inferredTypeClass = gart != null ? ((KindType)gart.GenericParameter.Returns).KindOf as TypeClass : null;
                            FieldInfo typeClassAccessor = null;

                            // TODO: order specializations to prevent dispatching to something that is just going to dispatch again?
                            var specificTargetType = Compile(specializationParam.SpecificFunctionParameter.Returns);
                            //gen.EmitWriteLine(string.Format("Checking specialization of {0} versus {1}", string.Join(" ", specializationParam.GeneralFunctionParameter.Takes), specificTargetType));
                            parameterCodes[specializationParam.GeneralFunctionParameter].Accessor(gen);
                            //
                            // if param.GetType() != specificType
                            //
                            specialCasts.Add(specializationParam.GeneralFunctionParameter, specificTargetType);

                            //
                            // RMS: This call would be better as a Constrained opcode, but that requires a ldarga (ptr load) not a ldarg (value load), but we 
                            //       don't know our parameter index at this point. Consider refactoring for perf.
                            //
                            gen.Emit(OpCodes.Box, Compile(specializationParam.GeneralFunctionParameter.RequiredArgumentType));

                            if (inferredTypeClass != null) {
                                Label skipUnboxingVariant = gen.DefineLabel();
                                
                                Compile(inferredTypeClass);
                                gen.Emit(OpCodes.Isinst, Compile(inferredTypeClass));
                                gen.Emit(OpCodes.Stloc, unboxingNeeded);
                                parameterCodes[specializationParam.GeneralFunctionParameter].Accessor(gen);
                                gen.Emit(OpCodes.Box, Compile(specializationParam.GeneralFunctionParameter.RequiredArgumentType));
                                gen.Emit(OpCodes.Ldloc, unboxingNeeded);
                                gen.Emit(OpCodes.Brfalse, skipUnboxingVariant);
                                typeClassAccessor = variantValueLookup[inferredTypeClass];
                                gen.Emit(OpCodes.Ldfld, typeClassAccessor);

                                gen.MarkLabel(skipUnboxingVariant);
                            }
                            
                            gen.Emit(OpCodes.Callvirt, objGetType);
                            //gen.EmitWriteLine("Specialization GetType success.");
                            gen.Emit(OpCodes.Ldtoken, specificTargetType);
                            gen.Emit(OpCodes.Call, getTypeFromHandle);
                            //gen.EmitWriteLine("GetTypeFromHandleSuccess");
                            gen.Emit(OpCodes.Call, typeEquality);
                            gen.Emit(OpCodes.Brfalse, next);

                            break;

                        case DispatchType.PartialSpecialization:
                            var specificPartialTargetType = Compile(specializationParam.SpecificFunctionParameter.RequiredArgumentType);
                            specificPartialTargetType = specificPartialTargetType.GetGenericTypeDefinition();
                            //gen.EmitWriteLine(string.Format("Checking specialization of {0} versus {1}", string.Join(" ", specializationParam.GeneralFunctionParameter.Takes), specificTargetType));
                            parameterCodes[specializationParam.GeneralFunctionParameter].Accessor(gen);

                            //
                            // if param.GetType().IsGenericType && param.GetType().GetGenericTypeDefinition() == specificType (partial specialization)
                            //
                            gen.Emit(OpCodes.Box, Compile(specializationParam.GeneralFunctionParameter.RequiredArgumentType));
                            gen.Emit(OpCodes.Callvirt, objGetType);
                            LocalBuilder paramTypeLocal;
                            if (!parameterTypeLocals.ContainsKey(specializationParam.GeneralFunctionParameter)) {
                                paramTypeLocal = gen.DeclareLocal(typeof(Type));
                                parameterTypeLocals.Add(specializationParam.GeneralFunctionParameter, paramTypeLocal);
                                gen.Emit(OpCodes.Stloc, paramTypeLocal);
                            } else {
                                paramTypeLocal = parameterTypeLocals[specializationParam.GeneralFunctionParameter];
                            }

                            gen.Emit(OpCodes.Ldloc, paramTypeLocal);
                            gen.Emit(OpCodes.Callvirt, isGenericType);
                            gen.Emit(OpCodes.Brfalse, next);

                            gen.Emit(OpCodes.Ldloc, paramTypeLocal);
                            gen.Emit(OpCodes.Callvirt, getGenericTypeDefinition);
                            gen.Emit(OpCodes.Ldtoken, specificPartialTargetType);
                            gen.Emit(OpCodes.Call, getTypeFromHandle);
                            gen.Emit(OpCodes.Call, typeEquality);
                            gen.Emit(OpCodes.Brfalse, next);

                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                Action<ParameterDeclaration, bool> emitParameterDispatch = (parameter, unbox) => {
                    if (modes.ContainsKey(parameter)) {
                        var valueFld = variantValueLookup[parameter.Returns];
                        parameterCodes[parameter].Accessor(gen);
                        gen.Emit(OpCodes.Ldfld, valueFld);

                        if (unbox) {
                            if (modes[parameter].Item2.IsValueType) {
                                gen.Emit(OpCodes.Unbox_Any, modes[parameter].Item2);
                            } else {
                                gen.Emit(OpCodes.Castclass, modes[parameter].Item2);
                            }
                        }
                    } else if (specialCasts.ContainsKey(parameter)) {
                        parameterCodes[parameter].Accessor(gen);
                        gen.Emit(OpCodes.Box, Compile(parameter.RequiredArgumentType));

                        var gart = (parameter.RequiredArgumentType as GenericArgumentReferenceType);
                        var inferredTypeClass = gart != null ? ((KindType)gart.GenericParameter.Returns).KindOf as TypeClass : null;
                        if (inferredTypeClass != null) {
                            var skipValueAccess = gen.DefineLabel();
                            gen.Emit(OpCodes.Isinst, Compile(inferredTypeClass));
                            gen.Emit(OpCodes.Stloc, unboxingNeeded);
                            parameterCodes[parameter].Accessor(gen);
                            gen.Emit(OpCodes.Box, Compile(parameter.RequiredArgumentType));
                            gen.Emit(OpCodes.Ldloc, unboxingNeeded);
                            gen.Emit(OpCodes.Brfalse, skipValueAccess);
                            var typeClassAccessor = variantValueLookup[inferredTypeClass];
                            gen.Emit(OpCodes.Ldfld, typeClassAccessor);
                            gen.MarkLabel(skipValueAccess);
                        }

                        if (unbox) {
                            if (specialCasts[parameter].IsValueType) {
                                gen.Emit(OpCodes.Unbox_Any, specialCasts[parameter]);
                            } else {
                                gen.Emit(OpCodes.Castclass, specialCasts[parameter]);
                            }
                        }
                    } else {
                        parameterCodes[parameter].Accessor(gen);
                        if (!unbox) {
                            gen.Emit(OpCodes.Box, Compile(parameter.RequiredArgumentType));
                            gen.Emit(OpCodes.Castclass, typeof(object));
                        }
                    }
                };

                // Cool. Load parameters, call function and return.
                if (specialization.GenericParameters.Any()) {
                    // Arg storage
                    var argArray = gen.DeclareLocal(typeof(object[]));

                    gen.Emit(OpCodes.Ldc_I4, fn.Takes.Where(pp => !pp.IsIdentifier).Count());
                    gen.Emit(OpCodes.Newarr, typeof(object));
                    gen.Emit(OpCodes.Stloc, argArray);

                    int ix = 0;
                    foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier)) {
                        gen.Emit(OpCodes.Ldloc, argArray);
                        gen.Emit(OpCodes.Ldc_I4, ix);
                        emitParameterDispatch(parameter.Parameter, false);
                        gen.Emit(OpCodes.Stelem_Ref);
                        ix++;
                    }

                    // Type params
                    var typeArgArray = gen.DeclareLocal(typeof(Type[]));

                    gen.Emit(OpCodes.Ldc_I4, specialization.GenericParameters.Count());
                    gen.Emit(OpCodes.Newarr, typeof(Type));
                    gen.Emit(OpCodes.Stloc, typeArgArray);

                    // Function to walk types and bind inferences:
                    Action<TangentType, Action> inferenceTypeWalker = null;
                    inferenceTypeWalker = new Action<TangentType, Action>((tt, typeAccessor) => {
                        switch (tt.ImplementationType) {
                            case KindOfType.BoundGeneric:
                                // List<T>, List<int>, something.
                                // Get args and work with them.
                                var genericArgArray = gen.DeclareLocal(typeof(Type[]));

                                typeAccessor();
                                gen.Emit(OpCodes.Callvirt, getGenericArguments);
                                gen.Emit(OpCodes.Stloc, genericArgArray);

                                int argumentIndex = 0;
                                foreach (var boundGenericArgument in ((BoundGenericType)tt).TypeArguments) {
                                    switch (boundGenericArgument.ImplementationType) {
                                        case KindOfType.BoundGeneric:
                                        case KindOfType.GenericReference:
                                            // Nested type.
                                            inferenceTypeWalker(boundGenericArgument, () => {
                                                gen.Emit(OpCodes.Ldloc, genericArgArray);
                                                gen.Emit(OpCodes.Ldc_I4, argumentIndex);
                                                gen.Emit(OpCodes.Ldelem_Ref);
                                            });

                                            break;

                                        default:
                                            // Something else. Probably a concrete bound argument.
                                            // Skip it.
                                            break;
                                    }

                                    argumentIndex++;
                                }

                                break;

                            case KindOfType.GenericReference:
                                // Awesome. What we're actually looking for add it to the type arg array.
                                // Load type array, target index, found type arg from array then store.
                                gen.Emit(OpCodes.Ldloc, typeArgArray);
                                gen.Emit(OpCodes.Ldc_I4, specialization.GenericParameters.IndexOf(((GenericArgumentReferenceType)tt).GenericParameter));

                                typeAccessor();

                                gen.Emit(OpCodes.Stelem_Ref);
                                break;

                            default:
                                // Something else. Probably a concrete bound argument.
                                // Skip it.
                                break;
                        }
                    });

                    // Now, bind them.
                    foreach (var partialSpecialization in specializationDetails.Where(s => s.SpecializationType == DispatchType.PartialSpecialization)) {
                        inferenceTypeWalker(partialSpecialization.SpecificFunctionParameter.RequiredArgumentType, () => {
                            // We already stored param.GetType() to a local. Use that.
                            gen.Emit(OpCodes.Ldloc, parameterTypeLocals[partialSpecialization.GeneralFunctionParameter]);
                        });
                    }

                    // Fix fn and go.
                    gen.Emit(OpCodes.Ldtoken, Compile(specialization));
                    gen.Emit(OpCodes.Call, getMethodFromHandle);
                    gen.Emit(OpCodes.Castclass, typeof(MethodInfo));
                    gen.Emit(OpCodes.Ldloc, typeArgArray);
                    gen.Emit(OpCodes.Callvirt, makeGenericMethod); // fn = fn<typeArgs>

                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ldloc, argArray);
                    gen.Emit(OpCodes.Call, invoke);  // fn.Invoke(null, args);
                    if (specialization.Returns.EffectiveType == TangentType.Void) {
                        gen.Emit(OpCodes.Pop);
                    } else {
                        // TODO: verify that return type isn't impacted by inference.
                        gen.Emit(OpCodes.Castclass, Compile(specialization.Returns.EffectiveType));
                    }
                } else {
                    foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier)) {
                        emitParameterDispatch(parameter.Parameter, true);
                    }

                    gen.Emit(OpCodes.Tailcall);
                    gen.EmitCall(OpCodes.Call, Compile(specialization), null);
                }

                gen.Emit(OpCodes.Ret);

                // Otherwise, place next label for next specialization (or global version).
                gen.Emit(OpCodes.Nop);
                gen.MarkLabel(next);
            }
        }

        private static int GetVariantMode(Type variantType, Type targetType)
        {
            var genericParams = variantType.BaseType.GetGenericArguments();
            for (int i = 1; i <= genericParams.Length; ++i) {
                if (genericParams[i - 1].AssemblyQualifiedName == targetType.AssemblyQualifiedName) {
                    return i;
                }
            }

            throw new ApplicationException(string.Format("Looking for {0} in variant {1}, but not found!", targetType, variantType));
        }

        private void AddDebuggingInfo(ILGenerator gen, Expression expr)
        {
            if (expr.SourceInfo != null) {
                System.Diagnostics.Debug.WriteLine(expr.SourceInfo);
                gen.MarkSequencePoint(debuggingSymbolLookup[expr.SourceInfo.Label], expr.SourceInfo.StartPosition.Line, expr.SourceInfo.StartPosition.Column, expr.SourceInfo.EndPosition.Line, expr.SourceInfo.EndPosition.Column);
            }
        }

        private void AddFunctionCode(ILGenerator gen, Block implementation, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes, ClosureInfo closureScope)
        {
            var statements = implementation.Statements.ToList();
            for (int ix = 0; ix < statements.Count; ++ix) {
                var stmt = statements[ix];
                AddDebuggingInfo(gen, stmt);
                AddExpression(stmt, gen, parameterCodes, closureScope, ix == statements.Count - 1);
            }
        }

        private void AddExpression(Expression expr, ILGenerator gen, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes, ClosureInfo closureScope, bool lastStatement)
        {
            switch (expr.NodeType) {
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    AddDebuggingInfo(gen, expr);
                    foreach (var p in invoke.Arguments) {
                        AddExpression(p, gen, parameterCodes, closureScope, false);
                    }

                    var ctor = invoke.FunctionDefinition.Returns as CtorCall;
                    if (ctor != null) {
                        var ctorParamTypes = invoke.Arguments.Select(a => Compile(a.EffectiveType)).ToArray();
                        var ctorType = Compile(invoke.EffectiveType);
                        ConstructorInfo ctorFn;

                        switch (invoke.EffectiveType.ImplementationType) {
                            case KindOfType.Product:
                            case KindOfType.BoundGeneric:
                                ctorFn = productCtorLookup[invoke.EffectiveType];
                                break;
                            case KindOfType.TypeClass:
                                ctorFn = variantCtorLookup[invoke.EffectiveType][ctorParamTypes.First()];
                                break;
                            default:
                                throw new NotImplementedException("Unexpected type in CtorCall");
                        }

                        gen.Emit(OpCodes.Newobj, ctorFn);
                        return;
                    }

                    // TODO: move these to exprs.
                    var upcast = invoke.FunctionDefinition.Returns as InterfaceUpcast;
                    if (upcast != null) {
                        // for now, it is just a sumtype built by the compiler.
                        var ctorParamTypes = invoke.Arguments.Select(a => Compile(a.EffectiveType)).ToArray();
                        Compile(invoke.EffectiveType);
                        var ctorFn = variantCtorLookup[invoke.EffectiveType][ctorParamTypes.First()];
                        gen.Emit(OpCodes.Newobj, ctorFn);
                        return;
                    }

                    var opcode = invoke.FunctionDefinition.Returns as Tangent.Intermediate.Interop.DirectOpCode;
                    if (opcode != null) {
                        gen.Emit(opcode.OpCode);
                        return;
                    }

                    // else, MethodInfo invocation.
                    if (lastStatement) { gen.Emit(OpCodes.Tailcall); }
                    if (invoke.GenericArguments.Any()) {
                        var parameterizedFn = Compile(invoke.FunctionDefinition).MakeGenericMethod(invoke.GenericArguments.Select(a => Compile(a)).ToArray());
                        gen.EmitCall(OpCodes.Call, parameterizedFn, null);
                    } else {
                        gen.EmitCall(OpCodes.Call, Compile(invoke.FunctionDefinition), null);
                    }

                    return;

                case ExpressionNodeType.CtorCall:
                    var ctorExpr = expr as CtorCallExpression;

                    var ctorExprParamTypes = ctorExpr.Arguments.Select(a => Compile(a.EffectiveType)).ToArray();
                    var ctorExprType = Compile(ctorExpr.EffectiveType);
                    ConstructorInfo ctorExprFn;

                    foreach (var p in ctorExpr.Arguments) {
                        AddExpression(p, gen, parameterCodes, closureScope, false);
                    }

                    switch (ctorExpr.EffectiveType.ImplementationType) {
                        case KindOfType.Product:
                        case KindOfType.BoundGeneric:
                            ctorExprFn = productCtorLookup[ctorExpr.EffectiveType];
                            break;
                        case KindOfType.TypeClass:
                            ctorExprFn = variantCtorLookup[ctorExpr.EffectiveType][ctorExprParamTypes.First()];
                            break;
                        default:
                            throw new NotImplementedException("Unexpected type in CtorCall");
                    }

                    gen.Emit(OpCodes.Newobj, ctorExprFn);
                    return;

                case ExpressionNodeType.FieldAccessor:
                    var fieldAccess = expr as FieldAccessorExpression;
                    if (fieldAccess.OwningType != null) {
                        var ot = Compile(fieldAccess.OwningType);
                        gen.Emit(OpCodes.Ldarg_0);
                        if (fieldAccess.OwningType.ImplementationType == KindOfType.BoundGeneric) {
                            gen.Emit(OpCodes.Ldfld, TypeBuilder.GetField(ot, fieldLookup[fieldAccess.TargetField]));
                        } else {
                            gen.Emit(OpCodes.Ldfld, fieldLookup[fieldAccess.TargetField]);
                        }
                    } else {
                        gen.Emit(OpCodes.Ldsfld, fieldLookup[fieldAccess.TargetField]);
                    }

                    return;

                case ExpressionNodeType.FieldMutator:
                    var fieldMutator = expr as FieldMutatorExpression;
                    if (fieldMutator.OwningType != null) {
                        var ot = Compile(fieldMutator.OwningType);
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldarg_1);

                        if (fieldMutator.OwningType.ImplementationType == KindOfType.BoundGeneric) {
                            gen.Emit(OpCodes.Stfld, TypeBuilder.GetField(ot, fieldLookup[fieldMutator.TargetField]));
                        } else {
                            gen.Emit(OpCodes.Stfld, fieldLookup[fieldMutator.TargetField]);
                        }
                    } else {
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Stsfld, fieldLookup[fieldMutator.TargetField]);
                    }

                    return;

                case ExpressionNodeType.Identifier:
                    throw new NotImplementedException("Bare identifier in compilation?");

                case ExpressionNodeType.ParameterAccess:
                    var access = (ParameterAccessExpression)expr;
                    parameterCodes[access.Parameter].Accessor(gen);

                    return;

                case ExpressionNodeType.DelegateInvocation:
                    var invocation = (DelegateInvocationExpression)expr;
                    AddExpression(invocation.DelegateAccess, gen, parameterCodes, closureScope, false);
                    foreach (var entry in invocation.Arguments) {
                        AddExpression(entry, gen, parameterCodes, closureScope, false);
                    }

                    var instanceType = Compile(invocation.DelegateType);
                    MethodInfo delegateInvoke = null;
                    if (instanceType.GetType().Name.StartsWith("TypeBuilder")) {
                        var genericType = GenericDelegateTypeFor(invocation.DelegateType);
                        delegateInvoke = TypeBuilder.GetMethod(instanceType, genericType.GetMethod("Invoke"));
                    } else {
                        delegateInvoke = instanceType.GetMethod("Invoke");
                    }

                    gen.EmitCall(OpCodes.Call, delegateInvoke, null);

                    return;

                case ExpressionNodeType.TypeAccess:
                    throw new NotImplementedException();

                case ExpressionNodeType.Unknown:
                    throw new NotImplementedException();

                case ExpressionNodeType.Constant:
                    var constant = (ConstantExpression)expr;
                    if (constant.EffectiveType == TangentType.String) {
                        gen.Emit(OpCodes.Ldstr, (string)constant.Value);
                        return;
                    } else if (constant.EffectiveType == TangentType.Int) {
                        gen.Emit(OpCodes.Ldc_I4, (int)constant.Value);
                        return;
                    } else {
                        throw new NotImplementedException();
                    }

                case ExpressionNodeType.EnumValueAccess:
                    var eva = (EnumValueAccessExpression)expr;
                    gen.Emit(OpCodes.Ldc_I4, eva.EnumValue.NumericEquivalent);
                    return;

                case ExpressionNodeType.EnumWidening:
                    var widening = (EnumWideningExpression)expr;
                    AddExpression(widening.EnumAccess, gen, parameterCodes, closureScope, lastStatement);
                    return;

                case ExpressionNodeType.CtorParamAccess:
                    var ctorAccess = (CtorParameterAccessExpression)expr;
                    if (!parameterCodes.ContainsKey(ctorAccess.ThisParam)) {
                        // Assume we're in some initializer.
                        gen.Emit(OpCodes.Ldarg_0);
                    } else {
                        parameterCodes[ctorAccess.ThisParam].Accessor(gen);
                    }

                    var thisType = Compile(ctorAccess.ThisParam.Returns);
                    if (thisType.IsGenericType && !thisType.IsGenericTypeDefinition) {
                        gen.Emit(OpCodes.Ldfld, TypeBuilder.GetField(thisType, ctorParamLookup[ctorAccess.CtorParam]));
                    } else {
                        gen.Emit(OpCodes.Ldfld, ctorParamLookup[ctorAccess.CtorParam]);
                    }
                    return;

                case ExpressionNodeType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    BuildClosure(gen, lambda, closureScope);
                    return;

                case ExpressionNodeType.InvalidProgramException:
                    gen.ThrowException(typeof(InvalidOperationException));
                    return;

                case ExpressionNodeType.LocalAccess:
                    var localAccess = (LocalAccessExpression)expr;
                    parameterCodes[localAccess.Local].Accessor(gen);
                    return;

                case ExpressionNodeType.LocalAssignment:
                    var localAssignment = (LocalAssignmentExpression)expr;
                    parameterCodes[localAssignment.Local.Local].Mutator(gen, () => AddExpression(localAssignment.Value, gen, parameterCodes, closureScope, false));
                    return;

                case ExpressionNodeType.DirectBox:
                    var directBox = (DirectBoxingExpression)expr;
                    AddExpression(directBox.Target, gen, parameterCodes, closureScope, lastStatement);
                    gen.Emit(OpCodes.Box);
                    return;

                case ExpressionNodeType.DirectCast:
                    var directCast = (DirectCastExpression)expr;
                    AddExpression(directCast.Argument, gen, parameterCodes, closureScope, lastStatement);
                    gen.Emit(OpCodes.Castclass, Compile(directCast.TargetType));
                    return;

                case ExpressionNodeType.DirectCall:
                    var directCall = (DirectCallExpression)expr;
                    var thisGenericCount = 0;
                    foreach (var arg in directCall.Arguments) {
                        AddExpression(arg, gen, parameterCodes, closureScope, false);
                    }

                    if (lastStatement) { gen.Emit(OpCodes.Tailcall); }

                    if (!directCall.Target.IsStatic && directCall.Arguments.Any()) {
                        thisGenericCount = directCall.Arguments.First().EffectiveType.ContainedGenericReferences().Count();
                    }

                    if (directCall.GenericArguments.Skip(thisGenericCount).Any()) {
                        var parameterizedFn = directCall.Target.MakeGenericMethod(directCall.Arguments.Skip(thisGenericCount).Select(a => Compile(a.EffectiveType)).ToArray());
                        gen.EmitCall(OpCodes.Call, parameterizedFn, null);
                    } else {
                        if (thisGenericCount > 0) {
                            var directConcreteCall = TypeBuilder.GetMethod(Compile(directCall.Arguments.First().EffectiveType), directCall.Target);
                            gen.EmitCall(OpCodes.Call, directConcreteCall, null);
                        } else {
                            gen.EmitCall(OpCodes.Call, directCall.Target, null);
                        }
                    }

                    return;

                case ExpressionNodeType.DirectConstructorCall:
                    var directCtor = (DirectConstructorCallExpression)expr;
                    foreach (var arg in directCtor.Arguments) {
                        AddExpression(arg, gen, parameterCodes, closureScope, false);
                    }

                    var genericArguments = directCtor.GenericArguments.Select(ga => Compile(GenericArgumentReferenceType.For(((GenericParameterAccessExpression)ga).Parameter))).ToList();
                    if (genericArguments.Any()) {
                        var concreteType = directCtor.Constructor.DeclaringType.MakeGenericType(genericArguments.ToArray());
                        gen.Emit(OpCodes.Newobj, TypeBuilder.GetConstructor(concreteType, directCtor.Constructor));
                    } else {
                        gen.Emit(OpCodes.Newobj, directCtor.Constructor);
                    }
                    return;

                case ExpressionNodeType.DirectFieldAccess:
                    var directFieldAccess = (DirectFieldAccessExpression)expr;
                    foreach (var arg in directFieldAccess.Arguments) {
                        AddExpression(arg, gen, parameterCodes, closureScope, false);
                    }

                    if (directFieldAccess.Field.IsStatic) {
                        gen.Emit(OpCodes.Ldsfld, directFieldAccess.Field);
                    } else {
                        gen.Emit(OpCodes.Ldfld, directFieldAccess.Field);
                    }

                    return;

                case ExpressionNodeType.DirectFieldAssignment:
                    var directFieldAssignment = (DirectFieldAssignmentExpression)expr;
                    foreach (var arg in directFieldAssignment.Arguments) {
                        AddExpression(arg, gen, parameterCodes, closureScope, false);
                    }

                    if (directFieldAssignment.Field.IsStatic) {
                        gen.Emit(OpCodes.Stsfld, directFieldAssignment.Field);
                    } else {
                        gen.Emit(OpCodes.Stfld, directFieldAssignment.Field);
                    }

                    return;

                case ExpressionNodeType.DirectStructInit:
                    var structInit = (DirectStructInitExpression)expr;
                    gen.Emit(OpCodes.Initobj, structInit.TargetStruct);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildClosure(ILGenerator gen, LambdaExpression lambda, ClosureInfo closureScope)
        {
            if (lambda.RequiresClosureImplementation()) {
                throw new NotImplementedException("TODO: support nested closures.");
            }

            bool needsCreating = false;

            if (closureScope == null) {
                // Then we have a lambda, but don't need any vars. Just toss it at root level for calling.
                var closureType = (rootType).DefineNestedType("closure" + closureCounter++, TypeAttributes.Sealed | TypeAttributes.NestedPublic);
                var cctor = closureType.DefineDefaultConstructor(MethodAttributes.Public);
                // TODO: if we use generics, do we need those here?
                closureScope = new ClosureInfo(closureType, new Dictionary<ParameterDeclaration, PropertyCodes>(), g => g.Emit(OpCodes.Newobj, cctor), Enumerable.Empty<ClosureGenericMapping>());
                needsCreating = true;
            }

            Type closureWithFnGenerics = closureScope.ClosureType;
            if (closureScope.ClosureGenericScope.Any()) {
                closureWithFnGenerics = closureScope.ClosureType.MakeGenericType(closureScope.ClosureGenericScope.Select(cgs => cgs.FunctionGeneric).ToArray());
            }

            // Here we need to overwrite the resolution for generic parameters since they're part of the generic instance, not the function.
            foreach (var entry in closureScope.ClosureGenericScope) {
                typeLookup[GenericArgumentReferenceType.For(entry.TangentGeneric)] = entry.ClosureGeneric;

                // And we need to get things like Func<T> too.
                var keys = new List<TangentType>();
                foreach (var kvp in typeLookup) {
                    if (kvp.Key != GenericArgumentReferenceType.For(entry.TangentGeneric) && kvp.Key.ContainedGenericReferences().Contains(entry.TangentGeneric)) {
                        keys.Add(kvp.Key);
                    }
                }

                foreach (var key in keys) {
                    typeLookup[key] = Compile(key);
                }
            }

            var nestedCodes = new Dictionary<ParameterDeclaration, PropertyCodes>(closureScope.ClosureCodes);

            // Build actual function
            var returnType = Compile(lambda.ResolvedReturnType);
            var parameterTypes = lambda.ResolvedParameters.Select(pd => Compile(pd.Returns)).ToArray();
            var closureFn = closureScope.ClosureType.DefineMethod("Implementation" + closureScope.ImplementationCounter++, System.Reflection.MethodAttributes.Public, returnType, parameterTypes);
            closureFn.SetReturnType(returnType);
            int ix = 1;
            foreach (var pd in lambda.ResolvedParameters) {
                var paramBuilder = closureFn.DefineParameter(ix++, ParameterAttributes.In, GetNameFor(pd));
                nestedCodes.Add(pd, new PropertyCodes(g => g.Emit(OpCodes.Ldarg, (Int16)ix - 1), null));
            }

            var closureGen = closureFn.GetILGenerator();

            // TODO: type resolve implementation bits?
            AddFunctionCode(closureGen, lambda.Implementation, nestedCodes, new ClosureInfo(closureScope.ClosureType, closureScope.ClosureCodes, g => g.Emit(OpCodes.Ldarg_0), closureScope.ClosureGenericScope));
            closureGen.Emit(OpCodes.Ret);

            // Push action creation onto stack.
            closureScope.ClosureAccessor(gen);
            if (closureScope.ClosureType.IsGenericTypeDefinition) {
                gen.Emit(OpCodes.Ldftn, TypeBuilder.GetMethod(closureWithFnGenerics, closureFn));
            } else {
                gen.Emit(OpCodes.Ldftn, closureFn);
            }

            var lambdaType = Compile(lambda.EffectiveType);
            ConstructorInfo ctor = null;

            if (lambdaType.GetType().Name.StartsWith("TypeBuilder")) {
                var genericFuncType = GenericDelegateTypeFor(lambda.EffectiveType as DelegateType);
                ctor = TypeBuilder.GetConstructor(lambdaType, genericFuncType.GetConstructors().First());
            } else {
                ctor = lambdaType.GetConstructors().First();
            }

            gen.Emit(OpCodes.Newobj, ctor);

            // And here we need to roll back the generic scope
            foreach (var entry in closureScope.ClosureGenericScope) {
                typeLookup[GenericArgumentReferenceType.For(entry.TangentGeneric)] = entry.FunctionGeneric;

                var keys = new List<TangentType>();
                foreach (var kvp in typeLookup) {
                    if (kvp.Key != GenericArgumentReferenceType.For(entry.TangentGeneric) && kvp.Key.ContainedGenericReferences().Contains(entry.TangentGeneric)) {
                        keys.Add(kvp.Key);
                    }
                }

                foreach (var key in keys) {
                    typeLookup[key] = Compile(key);
                }
            }

            if (needsCreating) {
                closureScope.ClosureType.CreateType();
            }
        }

        private string GetNameFor(TypeDeclaration rule, IEnumerable<TangentType> typeArguments = null)
        {
            typeArguments = typeArguments ?? Enumerable.Empty<TangentType>();

            if (rule.Takes.First() == null) {
                return "__AnonymousType" + anonymousTypeIndex++;
            }

            Func<PhrasePart, string> partPrinter = (pp) => {
                if (pp.IsIdentifier) { return pp.ToString(); }
                if (typeArguments.Any()) {
                    var s = GetNameFor(typeArguments.First());
                    typeArguments = typeArguments.Skip(1);
                    return s;
                }

                return pp.ToString();
            };

            string result = string.Join(" ", rule.Takes.Select(id => partPrinter(id)));
            var t = rule.Returns as DelegateType;
            while (t != null && !t.Takes.Any()) {
                result = "~> " + result;
                t = t.Returns as DelegateType;
            }

            return result;
        }

        private string GetNameFor(TangentType type)
        {
            if (type.ImplementationType == KindOfType.Delegate) {
                return GenericDelegateTypeFor((DelegateType)type).Name;
            }

            if (type.ImplementationType == KindOfType.BoundGeneric) {
                var boundType = (BoundGenericType)type;
                return GetNameFor(program.TypeDeclarations.FirstOrDefault(td => td.Returns == boundType.GenericType), boundType.TypeArguments);
            }

            if (type.ImplementationType == KindOfType.Kind) {
                return "kind of " + GetNameFor(((KindType)type).KindOf);
            }

            if (type.ImplementationType == KindOfType.GenericReference) {
                var reference = (GenericArgumentReferenceType)type;
                return string.Join(" ", reference.GenericParameter.Takes);
            }

            var decl = program.TypeDeclarations.FirstOrDefault(td => td.Returns == type);
            if (decl == null) {
                // For now, assume we're doing closure shenanigans.
                return "__AnonymousType" + anonymousTypeIndex++;
            }

            return GetNameFor(decl);
        }

        private string GetNameFor(ReductionDeclaration rule)
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

            sb.AppendFormat(" => {0}", GetNameFor(rule.Returns.EffectiveType));

            sb.Insert(0, string.Join(",", rule.GenericParameters.Select(pd => string.Format("<{0}>", string.Join(" ", pd.Takes)))));
            return sb.ToString();
        }


        private string GetNameFor(ParameterDeclaration rule)
        {
            string paramTypeName;
            if (rule.RequiredArgumentType.ImplementationType == KindOfType.SingleValue) {
                var svt = ((SingleValueType)rule.RequiredArgumentType);
                paramTypeName = GetNameFor(svt.ValueType) + "." + svt.Value.Value;
            } else {
                if (!rule.RequiredArgumentType.ContainedGenericReferences().Any()) {
                    paramTypeName = GetNameFor(rule.RequiredArgumentType);
                } else {
                    paramTypeName = "<inference>";
                }
            }

            string result = string.Join(" ", rule.Takes.Select(pp => pp.IsIdentifier ? pp.Identifier.Value : string.Format("({0})", GetNameFor(pp.Parameter.Returns)))) + ": " + paramTypeName;
            return result;
        }

        private Assembly OnTypeResolution(object sender, ResolveEventArgs args)
        {
            // TODO: optimization?

            foreach (var entry in typeLookup) {
                if (entry.Value.Name == args.Name) {
                    if (entry.Value is TypeBuilder) {
                        // WARNING: This is required, otherwise we return an AssemblyBuilder, which in turn yields a TypeLoadException on some CreateTypes (BasicADT causes it).
                        var type = (entry.Value as TypeBuilder).CreateType();
                        return type.Assembly;
                    }

                    return entry.Value.Assembly;
                }
            }

            return null;
        }


        public void Dispose()
        {
            this.compilationDomain.TypeResolve -= OnTypeResolution;
        }
    }
}
