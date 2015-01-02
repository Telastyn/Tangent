using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using N = System.Linq.Expressions;

namespace Tangent.CilGeneration
{
    public class CilCompiler
    {
        private readonly Dictionary<object, string> names = new Dictionary<object, string>();
        private readonly DebugInfoGenerator pdb = DebugInfoGenerator.CreatePdbGenerator();

        public void Compile(TangentProgram program, string targetPath)
        {
            string filename = Path.GetFileNameWithoutExtension(targetPath);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName(filename), AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(filename + ".dll", Path.GetFileName(targetPath) + ".dll", true);

            Dictionary<TangentType, Type> typeLookup = new Dictionary<TangentType, Type>() { { TangentType.Void, typeof(void) } };
            foreach (var type in program.TypeDeclarations.Where(td => td.Returns != TangentType.Void)) {
                var typeName = GetNameFor(type);
                var enumBuilder = moduleBuilder.DefineEnum(typeName, System.Reflection.TypeAttributes.Public, typeof(int));
                int x = 1;
                foreach (var value in type.Returns.Values) {
                    enumBuilder.DefineLiteral(value.Value, x++);
                }

                typeLookup.Add(type.Returns, enumBuilder.CreateType());
            }

            var rootClass = moduleBuilder.DefineType("_");

            Dictionary<ReductionDeclaration, MethodBuilder> fnLookup = new Dictionary<ReductionDeclaration, MethodBuilder>();
            foreach (var fn in program.Functions) {
                var fnName = GetNameFor(fn);
                var fnBuilder = rootClass.DefineMethod(
                    fnName,
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                    typeLookup[fn.Returns.EffectiveType],
                    fn.Takes.Where(t => !t.IsIdentifier).Select(t => typeLookup[t.Parameter.Returns]).ToArray());

                fnLookup.Add(fn, fnBuilder);
            }

            foreach (var fn in fnLookup) {
                BuildFunction(fn.Key, moduleBuilder, fn.Value, typeLookup, fnLookup);
            }

            rootClass.CreateType();

            assemblyBuilder.Save(targetPath + ".dll");
        }

        private void BuildFunction(ReductionDeclaration fn, ModuleBuilder mb, MethodBuilder fnBuilder, Dictionary<TangentType, Type> typeLookup, Dictionary<ReductionDeclaration, MethodBuilder> fnLookup)
        {
            var paramLookup = new Dictionary<ParameterDeclaration, UInt16>();
            UInt16 ix = 0;
            foreach (var param in fn.Takes.Where(t => !t.IsIdentifier)) {
                paramLookup.Add(param.Parameter, ix++);
            }

            var gen = fnBuilder.GetILGenerator();
            if (!fn.Returns.Implementation.Statements.Any()) {
                gen.Emit(OpCodes.Nop);
            } else {
                foreach (var statement in fn.Returns.Implementation.Statements) {
                    BuildStatement(statement, gen, paramLookup, fnLookup);
                }
            }
        }

        private void BuildStatement(Expression expr, ILGenerator gen, Dictionary<ParameterDeclaration, UInt16> paramLookup, Dictionary<ReductionDeclaration, MethodBuilder> fnLookup)
        {
            switch (expr.NodeType) {
                case ExpressionNodeType.FunctionBinding:
                    throw new NotImplementedException("Bare binding found when compiling statement.");
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    MethodBuilder mb = null;
                    if (fnLookup.TryGetValue(invoke.Bindings.FunctionDefinition, out mb)) {
                        // Simple function call. Go.
                        foreach (var p in invoke.Bindings.Parameters) {
                            BuildStatement(p, gen, paramLookup, fnLookup);
                        }

                        gen.EmitCall(OpCodes.Call, mb, null);
                    } else {
                        // We have some sort of anonymous function.
                        if (!invoke.Bindings.FunctionDefinition.Takes.Any()) {
                            // A non-parameterized function. For now, inline the thing.
                            var guts = invoke.Bindings.FunctionDefinition.Returns.Implementation.Statements;
                            foreach (var line in guts) {
                                BuildStatement(line, gen, paramLookup, fnLookup);
                            }
                        } else {
                            // An anonymous function with parameters.
                            // This currently can't occur. Throw.
                            throw new NotImplementedException("We have an anonymous function invocation with parameters?");
                        }
                    }

                    return;
                case ExpressionNodeType.Identifier:
                    throw new NotImplementedException("Bare Identifier found when compiling statement.");
                case ExpressionNodeType.ParameterAccess:
                    gen.Emit(OpCodes.Ldarg, paramLookup[((ParameterAccessExpression)expr).Parameter]);
                    return;
                case ExpressionNodeType.TypeAccess:
                    throw new NotImplementedException("Type constants are not currently supported.");
                case ExpressionNodeType.Unknown:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetNameFor(TypeDeclaration rule)
        {
            if (names.ContainsKey(rule)) {
                return names[rule];
            } else {
                string result = string.Join(" ", rule.Takes.Select(id => id.Value));
                names[rule] = result;
                return result;
            }
        }

        private string GetNameFor(ReductionDeclaration rule)
        {
            if (names.ContainsKey(rule)) {
                return names[rule];
            } else {
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

                // TODO: append result name since functions can vary by return type only.

                names[rule] = sb.ToString();
                return names[rule];
            }
        }

        private string GetNameFor(ParameterDeclaration rule)
        {
            if (names.ContainsKey(rule)) {
                return names[rule];
            } else {
                string result = string.Join(" ", rule.Takes.Select(id => id.Value));
                names[rule] = result;
                return result;
            }
        }
    }
}
