using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public class CilTypeCompiler:ITypeCompiler
    {
        private readonly ModuleBuilder builder;
        public CilTypeCompiler(ModuleBuilder builder)
        {
            this.builder = builder;
        }


        public Type Compile(TypeDeclaration typeDecl)
        {
            switch (typeDecl.Returns.ImplementationType) {
                case KindOfType.Enum:
                    return BuildEnum(typeDecl);
                default:
                    throw new NotImplementedException();
            }
        }

        private Type BuildEnum(TypeDeclaration result)
        {
            var typeName = GetNameFor(result);
            var enumBuilder = builder.DefineEnum(typeName, System.Reflection.TypeAttributes.Public, typeof(int));
            int x = 1;
            foreach (var value in (result.Returns as EnumType).Values) {
                enumBuilder.DefineLiteral(value.Value, x++);
            }

            return enumBuilder.CreateType();
        }

        public static string GetNameFor(TypeDeclaration rule)
        {
            string result = string.Join(" ", rule.Takes.Select(id => id.Value));
            var t = rule.Returns;
            while (t.ImplementationType == KindOfType.Lazy) {
                result = "~> " + result;
                t = (t as LazyType).Type;
            }

            return result;
        }

    }
}
