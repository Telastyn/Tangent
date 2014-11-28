using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class FunctionOrReductionDeclaration {
        public readonly ReductionDeclaration ReductionDeclaration;
        public readonly Function FunctionImplementation;

        public FunctionOrReductionDeclaration(ReductionDeclaration decl) {
            ReductionDeclaration = decl;
        }

        public FunctionOrReductionDeclaration(Function impl) {
            FunctionImplementation = impl;
        }


        public static implicit operator FunctionOrReductionDeclaration(Function type) {
            return new FunctionOrReductionDeclaration(type);
        }

        public static implicit operator FunctionOrReductionDeclaration(ReductionDeclaration decl) {
            return new FunctionOrReductionDeclaration(decl);
        }

        public void Dispatch(Action<Function> onFunction, Action<ReductionDeclaration> onDecl) {
            if (FunctionImplementation == null) {
                onDecl(ReductionDeclaration);
            } else {
                onFunction(FunctionImplementation);
            }
        }

        public T Dispatch<T>(Func<Function, T> onType, Func<ReductionDeclaration, T> onDecl) {
            if (FunctionImplementation == null) {
                return onDecl(ReductionDeclaration);
            } else {
                return onType(FunctionImplementation);
            }
        }
    }
}
