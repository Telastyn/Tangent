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
        public CilFunctionCompiler(IFunctionLookup builtins)
        {
            this.builtins = builtins;
        }

        public void BuildFunctionImplementation(ReductionDeclaration fn, MethodBuilder target, IEnumerable<ReductionDeclaration> specializations, TypeBuilder closureScope, IFunctionLookup fnLookup, ITypeLookup typeLookup)
        {
            fnLookup = new FallbackCompositeFunctionLookup(fnLookup, builtins);
            var gen = target.GetILGenerator();
            var parameterCodes = fn.Takes.Where(pp => !pp.IsIdentifier).Select((pp, ix) => new KeyValuePair<ParameterDeclaration, Action>(pp.Parameter, () => gen.Emit(OpCodes.Ldarg, (Int16)ix))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            LocalBuilder returnValue = null;
            Label returnLabel = gen.DefineLabel();

            if (target.ReturnType != typeof(void)) {
                gen.DeclareLocal(target.ReturnType);
            }

            AddDispatchCode(gen, fn, specializations, fnLookup, typeLookup, parameterCodes, returnValue, returnLabel);
            AddFunctionCode(gen, fn, fnLookup, typeLookup, parameterCodes, returnValue, returnLabel);
            AppendReturnCode(gen, returnValue, returnLabel);
        }

        private static void AddDispatchCode(ILGenerator gen, ReductionDeclaration fn, IEnumerable<ReductionDeclaration> specializations, IFunctionLookup fnLookup, ITypeLookup typeLookup, Dictionary<ParameterDeclaration, Action> parameterAccessors, LocalBuilder returnVariable, Label returnLabel)
        {
            if (!specializations.Any()) {
                return;
            }

            // For now, we can't have nested specializations, so just go in order, doing the checks.
            foreach (var specialization in specializations) {
                Label next = gen.DefineLabel();
                foreach (var typeValuePair in fn.Takes.Zip(specialization.Takes, Tuple.Create).Where(z => !z.Item2.IsIdentifier && z.Item2.Parameter.Returns.ImplementationType == KindOfType.SingleValue)) {
                    var single = (SingleValueType)typeValuePair.Item2.Parameter.Returns;
                    int v = 1;
                    foreach (var value in single.ValueType.Values) {
                        if (value == single.Value) {
                            break;
                        }

                        v++;
                    }

                    // If the specialization is not met, go to next specialization.
                    parameterAccessors[typeValuePair.Item1.Parameter]();
                    gen.Emit(OpCodes.Ldc_I4, v);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, next);
                    // Otherwise, proceed to next param.
                }

                // Cool. Load parameters, call function and return.
                foreach (var parameter in fn.Takes.Where(pp => !pp.IsIdentifier)) {
                    parameterAccessors[parameter.Parameter]();
                }

                gen.EmitCall(OpCodes.Call, fnLookup[specialization], null);
                if (returnVariable != null) {
                    gen.Emit(OpCodes.Stloc, returnVariable);
                }

                gen.Emit(OpCodes.Br, returnLabel);

                // Otherwise, place next label for next specialization (or global version).
                gen.Emit(OpCodes.Nop);
                gen.MarkLabel(next);
            }
        }


        private void AddFunctionCode(ILGenerator gen, ReductionDeclaration fn, IFunctionLookup fnLookup, ITypeLookup typeLookup, Dictionary<ParameterDeclaration, Action> parameterCodes, LocalBuilder returnValue, Label returnLabel)
        {
            foreach (var stmt in fn.Returns.Implementation.Statements) {
                AddExpression(stmt, gen, fn, fnLookup, typeLookup, parameterCodes, returnValue, returnLabel);
            }
        }

        private void AddExpression(Expression expr, ILGenerator gen, ReductionDeclaration fn, IFunctionLookup fnLookup, ITypeLookup typeLookup, Dictionary<ParameterDeclaration, Action> parameterCodes, LocalBuilder returnVariable, Label returnLabel)
        {
            switch (expr.NodeType) {
                case ExpressionNodeType.DelegateInvocation:
                    // Some parameter action needs invoked.
                    var dinvoke = (DelegateInvocationExpression)expr;
                    AddExpression(dinvoke.Delegate, gen, fn, fnLookup, typeLookup, parameterCodes, returnVariable, returnLabel);
                    gen.EmitCall(OpCodes.Call, typeLookup[((ParameterAccessExpression)dinvoke.Delegate).Parameter.Returns].GetMethod("Invoke", new Type[0]), null);
                    return;

                case ExpressionNodeType.FunctionBinding:
                    // Block?
                    throw new NotImplementedException();

                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    foreach (var p in invoke.Bindings.Parameters) {
                        AddExpression(p, gen, fn, fnLookup, typeLookup, parameterCodes, returnVariable, returnLabel);
                    }

                    gen.EmitCall(OpCodes.Call, fnLookup[invoke.Bindings.FunctionDefinition], null);
                    return;

                case ExpressionNodeType.HalfBoundExpression:
                    throw new NotImplementedException("Half bound expression in compilation?");

                case ExpressionNodeType.Identifier:
                    throw new NotImplementedException("Bare identifier in compilation?");

                case ExpressionNodeType.ParameterAccess:
                    var access = (ParameterAccessExpression)expr;
                    parameterCodes[access.Parameter]();
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
                    } else {
                        throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static void AppendReturnCode(ILGenerator gen, LocalBuilder returnValue, Label returnLabel)
        {
            if (returnValue != null) {
                gen.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new Type[0]));
                gen.Emit(OpCodes.Throw);
            }

            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(returnLabel);

            if (returnValue != null) {
                gen.Emit(OpCodes.Ldloc, returnValue);
            }

            gen.Emit(OpCodes.Ret);
        }
    }
}
