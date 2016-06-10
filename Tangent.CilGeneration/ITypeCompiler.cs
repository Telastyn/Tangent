using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.CilGeneration
{
    public interface ITypeCompiler
    {
        Type Compile(TypeDeclaration typeDecl, Action<TangentType, Type> placeholder, Func<TangentType, bool, Type> lookup, Action<Field, System.Reflection.FieldInfo> onFieldCreation);
    }
}
