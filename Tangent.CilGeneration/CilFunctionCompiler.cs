using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class CilFunctionCompiler : IFunctionCompiler
    {
        // TODO: pdb info.
        private readonly IFunctionLookup builtins;
        private int closureCounter = 1;
        public CilFunctionCompiler(IFunctionLookup builtins)
        {
            this.builtins = builtins;
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
            if (!specializations.Any())
            {
                return;
            }

            // For now, we can't have nested specializations, so just go in order, doing the checks.
            foreach (var specialization in specializations)
            {
                Label next = gen.DefineLabel();
                foreach (var typeValuePair in fn.Takes.Zip(specialization.Takes, Tuple.Create).Where(z => !z.Item2.IsIdentifier && z.Item2.Parameter.Returns.ImplementationType == KindOfType.SingleValue))
                {
                    var single = (SingleValueType)typeValuePair.Item2.Parameter.Returns;

                    // If the specialization is not met, go to next specialization.
                    parameterAccessors[typeValuePair.Item1.Parameter](gen);
                    gen.Emit(OpCodes.Ldc_I4, single.NumericEquivalent);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, next);
                    // Otherwise, proceed to next param.
                }

                // Cool. Load parameters, call function and return.
                foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier))
                {
                    parameterAccessors[parameter.Parameter](gen);
                }

                gen.EmitCall(OpCodes.Call, fnLookup[specialization], null);
                gen.Emit(OpCodes.Ret);

                // Otherwise, place next label for next specialization (or global version).
                gen.Emit(OpCodes.Nop);
                gen.MarkLabel(next);
            }
        }


        private void AddFunctionCode(ILGenerator gen, Block impl, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes)
        {
            foreach (var stmt in impl.Statements)
            {
                AddExpression(stmt, gen, fnLookup, typeLookup, closureScope, parameterCodes);
            }
        }

        private void AddExpression(Expression expr, ILGenerator gen, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes)
        {
            switch (expr.NodeType)
            {
                case ExpressionNodeType.DelegateInvocation:
                    // Some parameter action needs invoked.
                    var dinvoke = (DelegateInvocationExpression)expr;
                    AddExpression(dinvoke.Delegate, gen, fnLookup, typeLookup, closureScope, parameterCodes);
                    gen.EmitCall(OpCodes.Call, typeLookup[((ParameterAccessExpression)dinvoke.Delegate).Parameter.Returns].GetMethod("Invoke", new Type[0]), null);
                    return;

                case ExpressionNodeType.FunctionBinding:

                    var binding = (FunctionBindingExpression)expr;
                    // Closure time.
                    BuildClosure(gen, binding, fnLookup, typeLookup, closureScope, parameterCodes);

                    return;

                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;

                    foreach (var p in invoke.Bindings.Parameters)
                    {
                        AddExpression(p, gen, fnLookup, typeLookup, closureScope, parameterCodes);
                    }

                    gen.EmitCall(OpCodes.Call, fnLookup[invoke.Bindings.FunctionDefinition], null);

                    return;

                case ExpressionNodeType.Identifier:
                    throw new NotImplementedException("Bare identifier in compilation?");

                case ExpressionNodeType.ParameterAccess:
                    var access = (ParameterAccessExpression)expr;
                    parameterCodes[access.Parameter](gen);
                    return;

                case ExpressionNodeType.TypeAccess:
                    throw new NotImplementedException();

                case ExpressionNodeType.Unknown:
                    throw new NotImplementedException();

                case ExpressionNodeType.Constant:
                    var constant = (ConstantExpression)expr;
                    if (constant.EffectiveType == TangentType.String)
                    {
                        gen.Emit(OpCodes.Ldstr, (string)constant.Value);
                        return;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                case ExpressionNodeType.EnumValueAccess:
                    var eva = (EnumValueAccessExpression)expr;
                    gen.Emit(OpCodes.Ldc_I4, eva.EnumValue.NumericEquivalent);
                    return;

                case ExpressionNodeType.EnumWidening:
                    var widening = (EnumWideningExpression)expr;
                    AddExpression(widening.EnumAccess, gen, fnLookup, typeLookup, closureScope, parameterCodes);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Build closure, pushing the action onto the top of the stack.
        /// </summary>
        private void BuildClosure(ILGenerator gen, FunctionBindingExpression binding, IFunctionLookup fnLookup, ITypeLookup typeLookup, TypeBuilder closureScope, Dictionary<ParameterDeclaration, Action<ILGenerator>> parameterCodes)
        {
            // Build the anonymous type.
            var closure = closureScope.DefineNestedType("closure" + closureCounter++);
            var closureCtor = closure.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public);

            // Push type creation and parameter assignment onto the stack.
            var obj = gen.DeclareLocal(closure);
            gen.Emit(OpCodes.Newobj, closureCtor);
            gen.Emit(OpCodes.Stloc, obj);

            var closureReferences = new Dictionary<ParameterDeclaration, Action<ILGenerator>>();
            foreach (var parameter in parameterCodes)
            {
                gen.Emit(OpCodes.Ldloc, obj);
                var fld = closure.DefineField(CilScope.GetNameFor(parameter.Key, typeLookup), typeLookup[parameter.Key.Returns], System.Reflection.FieldAttributes.Public);
                parameter.Value(gen);
                gen.Emit(OpCodes.Stfld, fld);

                closureReferences.Add(parameter.Key, g => { g.Emit(OpCodes.Ldarg_0); g.Emit(OpCodes.Ldfld, fld); });
            }

            // Build actual function in anonymous type.
            var closureFn = closure.DefineMethod("Implementation", System.Reflection.MethodAttributes.Public);
            var closureGen = closureFn.GetILGenerator();

            // TODO: handle closure return properly.
            if (!binding.FunctionDefinition.Takes.Any())
            {
                // Block.
                AddFunctionCode(closureGen, binding.FunctionDefinition.Returns.Implementation, fnLookup, typeLookup, closure, closureReferences);
            }
            else
            {
                // Bound function.
                AddExpression(new FunctionInvocationExpression(binding), closureGen, fnLookup, typeLookup, closure, closureReferences);
            }

            closure.CreateType();

            // Push action creation onto stack.
            gen.Emit(OpCodes.Ldloc, obj);
            gen.Emit(OpCodes.Ldftn, closureFn);
            gen.Emit(OpCodes.Newobj, typeof(Action).GetConstructors().First());
        }
    }
}
