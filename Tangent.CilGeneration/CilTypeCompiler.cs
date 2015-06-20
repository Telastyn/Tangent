using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Runtime;

namespace Tangent.CilGeneration
{
    public class CilTypeCompiler : ITypeCompiler
    {
        private static int anonymousTypeIndex = 1;
        private readonly ModuleBuilder builder;
        public CilTypeCompiler(ModuleBuilder builder)
        {
            this.builder = builder;
        }

        public Type Compile(TypeDeclaration typeDecl, Action<Type> placeholder, Func<TangentType, bool, Type> lookup)
        {
            switch (typeDecl.Returns.ImplementationType) {
                case KindOfType.Enum:
                    return BuildEnum(typeDecl, placeholder, lookup);
                case KindOfType.Product:
                    return BuildClass(typeDecl, placeholder, lookup);
                case KindOfType.Sum:
                    return BuildVariant(typeDecl, placeholder, lookup);
                default:
                    throw new NotImplementedException();
            }
        }

        private Type BuildEnum(TypeDeclaration target, Action<Type> placeholder, Func<TangentType, bool, Type> lookup)
        {
            var typeName = GetNameFor(target);
            var enumBuilder = builder.DefineEnum(typeName, System.Reflection.TypeAttributes.Public, typeof(int));
            int x = 1;
            foreach (var value in (target.Returns as EnumType).Values) {
                enumBuilder.DefineLiteral(value.Value, x++);
            }

            return enumBuilder.CreateType();
        }

        private Type BuildClass(TypeDeclaration target, Action<Type> placeholder, Func<TangentType, bool, Type> lookup)
        {
            var typeName = GetNameFor(target);
            var productType = (ProductType)target.Returns;
            var tangentCtorParams = productType.DataConstructorParts.Where(pp => !pp.IsIdentifier).ToList();
            var dotnetCtorParamTypes = tangentCtorParams.Select(pp => lookup(pp.Parameter.Returns, true)).ToList();
            var me = lookup(target.Returns, false);
            if (me != null) { return me; }
            var classBuilder = builder.DefineType(typeName, System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public);
            placeholder(classBuilder);
            var ctor = classBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, dotnetCtorParamTypes.ToArray());
            var gen = ctor.GetILGenerator();

            for (int ix = 0; ix < dotnetCtorParamTypes.Count; ++ix) {
                var field = classBuilder.DefineField(string.Join(" ", tangentCtorParams[ix].Parameter.Takes.Select(id => id.Value)), dotnetCtorParamTypes[ix], System.Reflection.FieldAttributes.Public | System.Reflection.FieldAttributes.InitOnly);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg, ix + 1);
                gen.Emit(OpCodes.Stfld, field);
            }

            gen.Emit(OpCodes.Ret);
            return classBuilder.CreateType();
        }

        private Type BuildVariant(TypeDeclaration target, Action<Type> placeholder, Func<TangentType, bool, Type> lookup)
        {
            var typeName = GetNameFor(target);
            var sumType = (SumType)target.Returns;
            var classBuilder = builder.DefineType(typeName);
            placeholder(classBuilder);
            var variantTypes = sumType.Types.Select(tt => lookup(tt, true)).OrderBy(t => t.Name).ToArray();
            var variantContainer = typeof(Variant<,>).Module.GetTypes().FirstOrDefault(t => t.Name.StartsWith("Variant`" + variantTypes.Length));
            if (variantContainer == null) {
                throw new ApplicationException("Error finding runtime variant type of size " + variantTypes.Length);
            }

            var parent = variantContainer.MakeGenericType(variantTypes);
            classBuilder.SetParent(parent);
            foreach(var variantType in variantTypes){
                var ctor = classBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, new[] { variantType });
                var gen = ctor.GetILGenerator();

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, parent.GetConstructor(new[] { variantType }));
                gen.Emit(OpCodes.Ret);
            }

            return classBuilder.CreateType();
        }

        public static string GetNameFor(TypeDeclaration rule)
        {
            if (rule.Takes.First() == null) {
                return "__AnonymousType" + anonymousTypeIndex++;
            }

            string result = string.Join(" ", rule.Takes.Select(id => id.ToString()));
            var t = rule.Returns;
            while (t.ImplementationType == KindOfType.Lazy) {
                result = "~> " + result;
                t = (t as LazyType).Type;
            }

            return result;
        }

    }
}
