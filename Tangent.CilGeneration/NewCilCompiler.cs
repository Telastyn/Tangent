﻿using System;
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
        private readonly Dictionary<ProductType, ConstructorInfo> productCtorLookup = new Dictionary<ProductType, ConstructorInfo>();
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
            foreach (var builtinFn in BuiltinFunctions.All.Select(fn => new { Decl = fn, MI = BuiltinFunctions.DotNetFunctionForBuiltin(fn) }).Where(fn => fn.MI != null)) {
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
                case KindOfType.Sum:
                case KindOfType.TypeClass:
                    typeDecl = program.TypeDeclarations.First(td => td.Returns == target);
                    return BuildVariant(typeDecl);
                case KindOfType.BoundGeneric:
                    return InstantiateGeneric(target as BoundGenericType);
                case KindOfType.Delegate:
                    return BuildDelegateType(target as DelegateType);
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
                    var genInf = GenericInferencePlaceholder.For(entry.Item1);
                    typeLookup.Add(genRef, entry.Item2);
                    typeLookup.Add(genInf, entry.Item2);
                }
            }

            stub.SetReturnType(Compile(fn.Returns.EffectiveType));
            stub.SetParameters(
                fn.Takes.Where(t => !t.IsIdentifier).Select(t =>
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
            var productType = (ProductType)decl.Returns;

            var classBuilder = targetModule.DefineType(typeName, System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public);
            typeLookup.Add(decl.Returns, classBuilder);

            if (decl.IsGeneric) {
                var genericRefs = decl.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter);
                var genericBuilders = classBuilder.DefineGenericParameters(genericRefs.Select(pd => GetNameFor(pd)).ToArray());
                foreach (var entry in genericRefs.Zip(genericBuilders, (pd, gb) => Tuple.Create(pd, gb))) {
                    typeLookup.Add(GenericArgumentReferenceType.For(entry.Item1), entry.Item2);
                    typeLookup.Add(GenericInferencePlaceholder.For(entry.Item1), entry.Item2);
                }
            }

            var tangentCtorParams = productType.DataConstructorParts.Where(pp => !pp.IsIdentifier).ToList();
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
                throw new NotImplementedException("Field initialization yet to do.");
            }

            gen.Emit(OpCodes.Ret);
            return classBuilder;
        }

        private Type BuildVariant(TypeDeclaration decl)
        {
            var typeName = GetNameFor(decl);
            var variantParts = new List<TangentType>();
            switch (decl.Returns.ImplementationType) {
                case KindOfType.Sum:
                    variantParts.AddRange(((SumType)decl.Returns).Types);
                    break;
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
            Type genericType = Compile(boundGeneric.GenericTypeDeclatation.Returns);
            var arguments = boundGeneric.TypeArguments.Select(tt => Compile(tt)).ToArray();
            return genericType.MakeGenericType(arguments);
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

        private void BuildImplementation(ReductionDeclaration fn, MethodBuilder builder)
        {
            var specializations = program.Functions.Where(other => other.IsSpecializationOf(fn)).ToList();
            var gen = builder.GetILGenerator();

            // TODO: refactor these codes once closures are more well baked.
            var parameterCodes = fn.Takes.Where(pp => !pp.IsIdentifier).Select((pp, ix) => new KeyValuePair<ParameterDeclaration, PropertyCodes>(pp.Parameter, new PropertyCodes(g => g.Emit(OpCodes.Ldarg, (Int16)ix), null))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            int localix = 0;
            foreach (var local in fn.Returns.Implementation.Locals) {
                gen.DeclareLocal(Compile(local.Returns));
                parameterCodes.Add(local, new PropertyCodes(g => g.Emit(OpCodes.Ldloc, localix), g => g.Emit(OpCodes.Stloc, localix)));
            }

            AddDispatchCode(gen, fn, specializations, parameterCodes);
            AddFunctionCode(gen, fn.Returns.Implementation, parameterCodes);
            if (!(fn.Returns is InterfaceFunction)) {
                gen.Emit(OpCodes.Ret);
            }
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
                            gen.Emit(OpCodes.Ldc_I4, single.NumericEquivalent);
                            gen.Emit(OpCodes.Ceq);
                            gen.Emit(OpCodes.Brfalse, next);
                            // Otherwise, proceed to next param.
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
                            var inferredTypeClass = (specializationParam.GeneralFunctionParameter.RequiredArgumentType as GenericInferencePlaceholder)?.TypeClassInferred;
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
                                Compile(inferredTypeClass);
                                typeClassAccessor = variantValueLookup[inferredTypeClass];
                                gen.Emit(OpCodes.Ldfld, typeClassAccessor);
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

                        var inferredTypeClass = (parameter.RequiredArgumentType as GenericInferencePlaceholder)?.TypeClassInferred;
                        if (inferredTypeClass != null) {
                            Compile(inferredTypeClass);
                            var typeClassAccessor = variantValueLookup[inferredTypeClass];
                            gen.Emit(OpCodes.Ldfld, typeClassAccessor);
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
                                        case KindOfType.InferencePoint:
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

                            case KindOfType.InferencePoint:
                                // Awesome. What we're actually looking for add it to the type arg array.
                                // Load type array, target index, found type arg from array then store.
                                gen.Emit(OpCodes.Ldloc, typeArgArray);
                                gen.Emit(OpCodes.Ldc_I4, specialization.GenericParameters.IndexOf(((GenericInferencePlaceholder)tt).GenericArgument));

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

        private void AddFunctionCode(ILGenerator gen, Block implementation, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes, TypeBuilder closureScope = null)
        {
            closureScope = closureScope ?? rootType;
            var statements = implementation.Statements.ToList();
            for (int ix = 0; ix < statements.Count; ++ix) {
                var stmt = statements[ix];
                AddDebuggingInfo(gen, stmt);
                AddExpression(stmt, gen, parameterCodes, closureScope, ix == statements.Count - 1);
            }
        }

        private void AddExpression(Expression expr, ILGenerator gen, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes, TypeBuilder closureScope, bool lastStatement)
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
                        Compile(invoke.EffectiveType);
                        ConstructorInfo ctorFn;

                        switch (invoke.EffectiveType.ImplementationType) {
                            case KindOfType.Product:
                                ctorFn = productCtorLookup[invoke.EffectiveType as ProductType];
                                break;
                            case KindOfType.Sum:
                            case KindOfType.TypeClass:
                                ctorFn = variantCtorLookup[invoke.EffectiveType][ctorParamTypes.First()];
                                break;
                            default:
                                throw new NotImplementedException("Unexpected type in CtorCall");
                        }

                        gen.Emit(OpCodes.Newobj, ctorFn);
                        return;
                    }

                    var upcast = invoke.FunctionDefinition.Returns as InterfaceUpcast;
                    if (upcast != null) {
                        // for now, it is just a sumtype built by the compiler.
                        var ctorParamTypes = invoke.Arguments.Select(a => Compile(a.EffectiveType)).ToArray();
                        Compile(invoke.EffectiveType);
                        var ctorFn = variantCtorLookup[invoke.EffectiveType][ctorParamTypes.First()];
                        gen.Emit(OpCodes.Newobj, ctorFn);
                        return;
                    }

                    var opcode = invoke.FunctionDefinition.Returns as DirectOpCode;
                    if (opcode != null) {
                        gen.Emit(opcode.OpCode);
                        return;
                    }

                    // TODO: move field things to expressions?
                    var fieldAccess = invoke.FunctionDefinition.Returns as FieldAccessorFunction;
                    if (fieldAccess != null) {
                        Compile(fieldAccess.OwningType);
                        gen.Emit(OpCodes.Ldfld, fieldLookup[fieldAccess.TargetField]);
                        return;
                    }

                    var fieldMutator = invoke.FunctionDefinition.Returns as FieldMutatorFunction;
                    if (fieldMutator != null) {
                        Compile(fieldMutator.OwningType);
                        gen.Emit(OpCodes.Stfld, fieldLookup[fieldMutator.TargetField]);
                        return;
                    }

                    // else, MethodInfo invocation.
                    if (lastStatement) { gen.Emit(OpCodes.Tailcall); }
                    if (invoke.GenericArguments.Any()) {
                        var parameterizedFn = Compile(invoke.FunctionDefinition).MakeGenericMethod(invoke.Arguments.Select(a => Compile(a.EffectiveType)).ToArray());
                        gen.EmitCall(OpCodes.Call, parameterizedFn, null);
                    } else {
                        gen.EmitCall(OpCodes.Call, Compile(invoke.FunctionDefinition), null);
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

                    gen.EmitCall(OpCodes.Call, Compile(invocation.DelegateType).GetMethod("Invoke"), null);

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
                    gen.Emit(OpCodes.Ldfld, ctorParamLookup[ctorAccess.CtorParam]);
                    return;

                case ExpressionNodeType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    BuildClosure(gen, lambda, parameterCodes, closureScope);
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
                    AddExpression(localAssignment.Value, gen, parameterCodes, closureScope, lastStatement);
                    parameterCodes[localAssignment.Local.Local].Mutator(gen);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildClosure(ILGenerator gen, LambdaExpression lambda, Dictionary<ParameterDeclaration, PropertyCodes> parameterCodes, TypeBuilder closureScope)
        {
            // Build the anonymous type.
            var closure = closureScope.DefineNestedType("closure" + closureCounter++);
            var closureCtor = closure.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public);

            // Push type creation and parameter assignment onto the stack.
            var obj = gen.DeclareLocal(closure);
            gen.Emit(OpCodes.Newobj, closureCtor);
            gen.Emit(OpCodes.Stloc, obj);

            var closureReferences = new Dictionary<ParameterDeclaration, PropertyCodes>();
            foreach (var parameter in parameterCodes) {
                gen.Emit(OpCodes.Ldloc, obj);
                var fld = closure.DefineField(GetNameFor(parameter.Key), Compile(parameter.Key.Returns), System.Reflection.FieldAttributes.Public);
                parameter.Value.Accessor(gen);
                gen.Emit(OpCodes.Stfld, fld);

                closureReferences.Add(parameter.Key, new PropertyCodes(g => { g.Emit(OpCodes.Ldarg_0); g.Emit(OpCodes.Ldfld, fld); }, null));
            }

            // Build actual function in anonymous type.
            var returnType = Compile(lambda.ResolvedReturnType);
            var parameterTypes = lambda.ResolvedParameters.Select(pd => Compile(pd.Returns)).ToArray();
            var closureFn = closure.DefineMethod("Implementation", System.Reflection.MethodAttributes.Public, returnType, parameterTypes);
            closureFn.SetReturnType(returnType);
            int ix = 1;
            foreach (var pd in lambda.ResolvedParameters) {
                var paramBuilder = closureFn.DefineParameter(ix++, ParameterAttributes.In, GetNameFor(pd));
                closureReferences.Add(pd, new PropertyCodes(g => g.Emit(OpCodes.Ldarg, (Int16)ix - 1), g => g.Emit(OpCodes.Starg, (Int16)ix - 1)));
            }

            var closureGen = closureFn.GetILGenerator();

            AddFunctionCode(closureGen, lambda.Implementation, closureReferences, closure);
            closureGen.Emit(OpCodes.Ret);

            closure.CreateType();

            // Push action creation onto stack.
            gen.Emit(OpCodes.Ldloc, obj);
            gen.Emit(OpCodes.Ldftn, closureFn);
            gen.Emit(OpCodes.Newobj, Compile(lambda.EffectiveType).GetConstructors().First());
        }

        private string GetNameFor(TypeDeclaration rule)
        {
            if (rule.Takes.First() == null) {
                return "__AnonymousType" + anonymousTypeIndex++;
            }

            string result = string.Join(" ", rule.Takes.Select(id => id.ToString()));
            var t = rule.Returns as DelegateType;
            while (t != null && !t.Takes.Any()) {
                result = "~> " + result;
                t = t.Returns as DelegateType;
            }

            return result;
        }

        private string GetNameFor(TangentType type)
        {
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
                if (!rule.RequiredArgumentType.ContainedGenericReferences(GenericTie.Inference).Any()) {
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
