using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class TypeOrTypeDeclaration {
        public readonly TangentType Type;
        public readonly TypeDeclaration TypeDeclaration;

        public TypeOrTypeDeclaration(TangentType type) {
            if (type == null) { throw new NullReferenceException(); }
            Type = type;
        }

        public TypeOrTypeDeclaration(TypeDeclaration decl) {
            if (decl == null) { throw new NullReferenceException(); }
            TypeDeclaration = decl;
        }

        public static implicit operator TypeOrTypeDeclaration(TangentType type) {
            return new TypeOrTypeDeclaration(type);
        }

        public static implicit operator TypeOrTypeDeclaration(TypeDeclaration decl) {
            return new TypeOrTypeDeclaration(decl);
        }

        public void Dispatch(Action<TangentType> onType, Action<TypeDeclaration> onDecl) {
            if (Type == null) {
                onDecl(TypeDeclaration);
            } else {
                onType(Type);
            }
        }

        public T Dispatch<T>(Func<TangentType, T> onType, Func<TypeDeclaration, T> onDecl) {
            if (Type == null) {
                return onDecl(TypeDeclaration);
            } else {
                return onType(Type);
            }
        }
    }
}
