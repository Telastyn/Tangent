using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class CilFunctionCompiler : IFunctionCompiler
    {
        private readonly Dictionary<string, ISymbolDocumentWriter> debuggingDocWriter;
        private readonly IFunctionLookup builtins;
        private int closureCounter = 1;
        public CilFunctionCompiler(IFunctionLookup builtins, Dictionary<string, ISymbolDocumentWriter> debuggingDocWriter)
        {
            this.builtins = builtins;
            this.debuggingDocWriter = debuggingDocWriter;
        }

        public void BuildFunctionImplementation(ReductionDeclaration fn, MethodBuilder target, IEnumerable<ReductionDeclaration> specializations, TypeBuilder closureScope, IFunctionLookup fnLookup, ITypeLookup typeLookup)
        {
            fnLookup = new FallbackCompositeFunctionLookup(fnLookup, builtins);
            var gen = target.GetILGenerator();
            var parameterCodes = fn.Takes.Where(pp => !pp.IsIdentifier).Select((pp, ix) => new KeyValuePair<ParameterDeclaration, Action<ILGenerator>>(pp.Parameter, g => g.Emit(OpCodes.Ldarg, (Int16)ix))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //gen.Emit(OpCodes.Break);
            AddDispatchCode(gen, fn, specializations, fnLookup, typeLookup, parameterCodes);
            AddFunctionCode(gen, fn.Returns.Implementation, fnLookup, typeLookup, closureScope, parameterCodes);
            gen.Emit(OpCodes.Ret);
        }

        private static void AddDispatchCode(ILGenerator gen, ReductionDeclaration fn, IEnumerable<ReductionDeclaration> specializations, IFunctionLookup fnLookup, ITypeLookup typeLookup, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterAccessors)
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
                            parameterAccessors[specializationParam.GeneralFunctionParameter](gen);
                            gen.Emit(OpCodes.Ldc_I4, single.NumericEquivalent);
                            gen.Emit(OpCodes.Ceq);
                            gen.Emit(OpCodes.Brfalse, next);
                            // Otherwise, proceed to next param.
                            break;

                        case DispatchType.SumType:
                            var dotNetSum = typeLookup[specializationParam.GeneralFunctionParameter.Returns];
                            var dotNetTarget = typeLookup[specializationParam.SpecificFunctionParameter.Returns];
                            var targetMode = GetVariantMode(dotNetSum, dotNetTarget);
                            modes.Add(specializationParam.GeneralFunctionParameter, Tuple.Create(dotNetSum, dotNetTarget));
                            var modeField = dotNetSum.GetField("Mode");
                            parameterAccessors[specializationParam.GeneralFunctionParameter](gen);
                            gen.Emit(OpCodes.Ldfld, modeField);
                            gen.Emit(OpCodes.Ldc_I4, targetMode);
                            gen.Emit(OpCodes.Ceq);
                            gen.Emit(OpCodes.Brfalse, next);
                            break;

                        case DispatchType.GenericSpecialization:
                            // TODO: order specializations to prevent dispatching to something that is just going to dispatch again?
                            var specificTargetType = typeLookup[specializationParam.SpecificFunctionParameter.Returns];
                            //gen.EmitWriteLine(string.Format("Checking specialization of {0} versus {1}", string.Join(" ", specializationParam.GeneralFunctionParameter.Takes), specificTargetType));
                            parameterAccessors[specializationParam.GeneralFunctionParameter](gen);


                            //
                            // if param.GetType() != specificType
                            //
                            specialCasts.Add(specializationParam.GeneralFunctionParameter, specificTargetType);
                            //
                            // RMS: This call would be better as a Constrained opcode, but that requires a ldarga (ptr load) not a ldarg (value load), but we 
                            //       don't know our parameter index at this point. Consider refactoring for perf.
                            //
                            gen.Emit(OpCodes.Box, typeLookup[specializationParam.GeneralFunctionParameter.RequiredArgumentType]);
                            gen.Emit(OpCodes.Callvirt, objGetType);
                            //gen.EmitWriteLine("Specialization GetType success.");
                            gen.Emit(OpCodes.Ldtoken, specificTargetType);
                            gen.Emit(OpCodes.Call, getTypeFromHandle);
                            //gen.EmitWriteLine("GetTypeFromHandleSuccess");
                            gen.Emit(OpCodes.Call, typeEquality);
                            gen.Emit(OpCodes.Brfalse, next);

                            break;

                        case DispatchType.PartialSpecialization:
                            var specificPartialTargetType = typeLookup[specializationParam.SpecificFunctionParameter.RequiredArgumentType];
                            specificPartialTargetType = specificPartialTargetType.GetGenericTypeDefinition();
                            //gen.EmitWriteLine(string.Format("Checking specialization of {0} versus {1}", string.Join(" ", specializationParam.GeneralFunctionParameter.Takes), specificTargetType));
                            parameterAccessors[specializationParam.GeneralFunctionParameter](gen);

                            //
                            // if param.GetType().IsGenericType && param.GetType().GetGenericTypeDefinition() == specificType (partial specialization)
                            //
                            gen.Emit(OpCodes.Box, typeLookup[specializationParam.GeneralFunctionParameter.RequiredArgumentType]);
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

                Action<ParameterDeclaration, bool> emitParameterDispatch = (parameter, unbox) =>
                {
                    if (modes.ContainsKey(parameter)) {
                        var valueFld = modes[parameter].Item1.GetField("Value");
                        parameterAccessors[parameter](gen);
                        gen.Emit(OpCodes.Ldfld, valueFld);

                        if (unbox) {
                            if (modes[parameter].Item2.IsValueType) {
                                gen.Emit(OpCodes.Unbox_Any, modes[parameter].Item2);
                            } else {
                                gen.Emit(OpCodes.Castclass, modes[parameter].Item2);
                            }
                        }
                    } else if (specialCasts.ContainsKey(parameter)) {
                        parameterAccessors[parameter](gen);
                        gen.Emit(OpCodes.Box, typeLookup[parameter.RequiredArgumentType]);
                        if (unbox) {
                            if (specialCasts[parameter].IsValueType) {
                                gen.Emit(OpCodes.Unbox_Any, specialCasts[parameter]);
                            } else {
                                gen.Emit(OpCodes.Castclass, specialCasts[parameter]);
                            }
                        }
                    } else {
                        parameterAccessors[parameter](gen);
                        if (!unbox) {
                            gen.Emit(OpCodes.Box, typeLookup[parameter.RequiredArgumentType]);
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
                    inferenceTypeWalker = new Action<TangentType, Action>((tt, typeAccessor) =>
                    {
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
                                            inferenceTypeWalker(boundGenericArgument, () =>
                                            {
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
                        inferenceTypeWalker(partialSpecialization.SpecificFunctionParameter.RequiredArgumentType, () =>
                        {
                            // We already stored param.GetType() to a local. Use that.
                            gen.Emit(OpCodes.Ldloc, parameterTypeLocals[partialSpecialization.GeneralFunctionParameter]);
                        });
                    }

                    // Fix fn and go.
                    gen.Emit(OpCodes.Ldtoken, fnLookup[specialization]);
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
                        gen.Emit(OpCodes.Castclass, typeLookup[specialization.Returns.EffectiveType]);
                    }
                } else {
                    foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier)) {
                        emitParameterDispatch(parameter.Parameter, true);
                    }

                    gen.Emit(OpCodes.Tailcall);
                    gen.EmitCall(OpCodes.Call, fnLookup[specialization], null);
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
                gen.MarkSequencePoint(debuggingDocWriter[expr.SourceInfo.Label], expr.SourceInfo.StartPosition.Line, expr.SourceInfo.StartPosition.Column, expr.SourceInfo.EndPosition.Line, expr.SourceInfo.EndPosition.Column);
            }
        }

        private void AddFunctionCode(ILGenerator gen, Block impl, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes)
        {
            var statements = impl.Statements.ToList();
            for (int ix = 0; ix < statements.Count; ++ix) {
                var stmt = statements[ix];
                AddDebuggingInfo(gen, stmt);
                AddExpression(stmt, gen, fnLookup, typeLookup, closureScope, parameterCodes, ix == statements.Count - 1);
            }
        }

        private void AddExpression(Expression expr, ILGenerator gen, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes, bool lastStatement)
        {
            switch (expr.NodeType) {
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    AddDebuggingInfo(gen, expr);
                    foreach (var p in invoke.Arguments) {
                        AddExpression(p, gen, fnLookup, typeLookup, closureScope, parameterCodes, false);
                    }

                    var ctor = invoke.FunctionDefinition.Returns as CtorCall;
                    if (ctor != null) {
                        var ctorParamTypes = invoke.Arguments.Select(a => typeLookup[a.EffectiveType]).ToArray();
                        var ctorFn = typeLookup[invoke.EffectiveType].GetConstructor(ctorParamTypes);
                        gen.Emit(OpCodes.Newobj, ctorFn);
                        return;
                    }

                    var interfaceFn = invoke.FunctionDefinition.Returns as InterfaceFunction;
                    if (interfaceFn != null) {
                        gen.ThrowException(typeof(NotImplementedException));
                        return;
                    }

                    var opcode = invoke.FunctionDefinition.Returns as DirectOpCode;
                    if (opcode != null) {
                        gen.Emit(opcode.OpCode);
                        return;
                    }

                    // else, MethodInfo invocation.
                    if (lastStatement) { gen.Emit(OpCodes.Tailcall); }
                    if (invoke.GenericArguments.Any()) {
                        var parameterizedFn = fnLookup[invoke.FunctionDefinition].MakeGenericMethod(invoke.Arguments.Select(a => typeLookup[a.EffectiveType]).ToArray());
                        gen.EmitCall(OpCodes.Call, parameterizedFn, null);
                    } else {
                        gen.EmitCall(OpCodes.Call, fnLookup[invoke.FunctionDefinition], null);
                    }

                    return;

                case ExpressionNodeType.Identifier:
                    throw new NotImplementedException("Bare identifier in compilation?");

                case ExpressionNodeType.ParameterAccess:
                    var access = (ParameterAccessExpression)expr;
                    parameterCodes[access.Parameter](gen);

                    return;

                case ExpressionNodeType.DelegateInvocation:
                    var invocation = (DelegateInvocationExpression)expr;
                    AddExpression(invocation.DelegateAccess, gen, fnLookup, typeLookup, closureScope, parameterCodes, false);
                    foreach (var entry in invocation.Arguments) {
                        AddExpression(entry, gen, fnLookup, typeLookup, closureScope, parameterCodes, false);
                    }

                    gen.EmitCall(OpCodes.Call, typeLookup[invocation.DelegateType].GetMethod("Invoke"), null);

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
                    AddExpression(widening.EnumAccess, gen, fnLookup, typeLookup, closureScope, parameterCodes, lastStatement);
                    return;

                case ExpressionNodeType.CtorParamAccess:
                    var ctorAccess = (CtorParameterAccessExpression)expr;
                    parameterCodes[ctorAccess.ThisParam](gen);
                    var thisType = typeLookup[ctorAccess.ThisParam.Returns];
                    gen.Emit(OpCodes.Ldfld, thisType.GetField(CilScope.GetNameFor(ctorAccess.CtorParam, typeLookup)));
                    return;

                case ExpressionNodeType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    BuildClosure(gen, lambda, fnLookup, typeLookup, closureScope, parameterCodes);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Build closure, pushing the action onto the top of the stack.
        /// </summary>
        private void BuildClosure(ILGenerator gen, LambdaExpression lambda, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes)
        {
            // Build the anonymous type.
            var closure = closureScope.DefineNestedType("closure" + closureCounter++);
            var closureCtor = closure.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public);

            // Push type creation and parameter assignment onto the stack.
            var obj = gen.DeclareLocal(closure);
            gen.Emit(OpCodes.Newobj, closureCtor);
            gen.Emit(OpCodes.Stloc, obj);

            var closureReferences = new Dictionary<ParameterDeclaration, Action<ILGenerator>>();
            foreach (var parameter in parameterCodes) {
                gen.Emit(OpCodes.Ldloc, obj);
                var fld = closure.DefineField(CilScope.GetNameFor(parameter.Key, typeLookup), typeLookup[parameter.Key.Returns], System.Reflection.FieldAttributes.Public);
                parameter.Value(gen);
                gen.Emit(OpCodes.Stfld, fld);

                closureReferences.Add(parameter.Key, g => { g.Emit(OpCodes.Ldarg_0); g.Emit(OpCodes.Ldfld, fld); });
            }

            // Build actual function in anonymous type.
            var returnType = typeLookup[lambda.ResolvedReturnType];
            var parameterTypes = lambda.ResolvedParameters.Select(pd => typeLookup[pd.Returns]).ToArray();
            var closureFn = closure.DefineMethod("Implementation", System.Reflection.MethodAttributes.Public, returnType, parameterTypes);
            closureFn.SetReturnType(returnType);
            int ix = 1;
            foreach (var pd in lambda.ResolvedParameters) {
                var paramBuilder = closureFn.DefineParameter(ix++, ParameterAttributes.In, CilScope.GetNameFor(pd, typeLookup));
                closureReferences.Add(pd, g => g.Emit(OpCodes.Ldarg, (Int16)ix-1));
            }

            var closureGen = closureFn.GetILGenerator();

            AddFunctionCode(closureGen, lambda.Implementation, fnLookup, typeLookup, closure, closureReferences);
            closureGen.Emit(OpCodes.Ret);

            closure.CreateType();

            // Push action creation onto stack.
            gen.Emit(OpCodes.Ldloc, obj);
            gen.Emit(OpCodes.Ldftn, closureFn);
            gen.Emit(OpCodes.Newobj, typeLookup[lambda.EffectiveType].GetConstructors().First());
        }
    }
}
