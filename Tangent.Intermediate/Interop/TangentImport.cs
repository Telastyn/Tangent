using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tangent.Tokenization;

namespace Tangent.Intermediate.Interop
{
    public static class TangentImport
    {
        private static readonly HashSet<string> DotNetOperatorNames = new HashSet<string>() {
            "op_Implicit",
            "op_Explicit",
            "op_Addition",
            "op_Subtraction",
            "op_Multiply",
            "op_Division",
            "op_Modulus",
            "op_ExclusiveOr",
            "op_BitwiseAnd",
            "op_BitwiseOr",
            "op_LogicalAnd",
            "op_LogicalOr",
            "op_LeftShift",
            "op_RightShift",
            "op_SignedRightShift",
            "op_UnsignedRightShift",
            "op_Equality",
            "op_GreaterThan",
            "op_LessThan",
            "op_Inequality",
            "op_GreaterThanOrEqual",
            "op_LessThanOrEqual",
            "op_UnaryNegation",
        };

        public static ImportBundle ImportAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Aggregate(new PartialImportBundle(), (r, a) => PartialImportBundle.Merge(r, ImportAssembly(a)));
        }

        public static PartialImportBundle ImportAssembly(Assembly assembly)
        {
            PartialImportBundle result = new PartialImportBundle();

            foreach (var t in assembly.GetTypes()) {
                // TODO: limit to public?
                var tangentType = DotNetType.For(t);
                if (tangentType != null) {
                    result.Types.Add(t, TypeDeclarationFor(t));

                    foreach (var fn in t.GetMethods()) {
                        var tfn = ReductionDeclarationFor(fn);
                        if (tfn != null) {
                            result.CommonFunctions.Add(fn, tfn);
                        }
                    }
                    // TODO: ctor, fn, prop, field, etc.
                }
            }

            return result;
        }

        public static TypeDeclaration TypeDeclarationFor(Type t)
        {
            var tt = DotNetType.For(t);
            if (t.ContainsGenericParameters) {
                throw new NotImplementedException("Sorry, generic dot net types not yet supported.");
            }

            var phrase = Tokenize.ProgramFile(".NET " + t.FullName, "").Select(token => new Identifier(token.Value)).ToList();

            return new TypeDeclaration(phrase, tt);
        }

        public static ReductionDeclaration ReductionDeclarationFor(MethodInfo fn)
        {
            if (fn.IsStatic) {
                return ReductionDeclarationForStatic(fn);
            }

            if (fn.IsGenericMethod) {
                // TODO:
                return null;
            }

            if (fn.IsSpecialName) {
                // A property? Event? Ctor? Punt for now.
                return null;
            }

            var owningTangentType = DotNetType.For(fn.DeclaringType);
            if (owningTangentType == null) { return null; }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            var phrase = new List<PhrasePart>();
            phrase.Add(new PhrasePart(new ParameterDeclaration("this", owningTangentType)));
            phrase.Add(new PhrasePart("."));
            phrase.Add(new PhrasePart(fn.Name));

            foreach (var parameter in fn.GetParameters()) {
                if (parameter.IsOut) {
                    return null;
                }

                var parameterType = DotNetType.For(parameter.ParameterType);
                if (parameterType == null) { return null; }
                phrase.Add(new PhrasePart(new ParameterDeclaration(parameter.Name, parameterType)));
            }

            return new ReductionDeclaration(phrase, new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, phrase.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), Enumerable.Empty<TangentType>()) }, Enumerable.Empty<ParameterDeclaration>())));

        }

        public static ReductionDeclaration ReductionDeclarationForStatic(MethodInfo fn)
        {
            if (!fn.IsStatic) {
                return null;
            }

            if (fn.IsGenericMethod) {
                // TODO:
                return null;
            }

            if (DotNetOperatorNames.Contains(fn.Name)) {
                return ReductionDeclarationForOperator(fn);
            }

            if (fn.IsSpecialName) {
                // A property? Event? Ctor? Punt for now.
                return null;
            }

            var owningTangentType = DotNetType.For(fn.DeclaringType);
            if (owningTangentType == null) { return null; }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            var namePart = Tokenize.ProgramFile(".NET " + fn.DeclaringType.FullName + "." + fn.Name, "").Select(token => new Identifier(token.Value)).ToList();
            List<PhrasePart> phrase = new List<PhrasePart>(namePart.Select(id => new PhrasePart(id)));

            foreach (var parameter in fn.GetParameters()) {
                if (parameter.IsOut) {
                    return null;
                }

                var parameterType = DotNetType.For(parameter.ParameterType);
                if (parameterType == null) { return null; }
                phrase.Add(new PhrasePart(new ParameterDeclaration(parameter.Name, parameterType)));
            }

            return new ReductionDeclaration(phrase, new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, phrase.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), Enumerable.Empty<TangentType>()) }, Enumerable.Empty<ParameterDeclaration>())));
        }

        public static ReductionDeclaration ReductionDeclarationForOperator(MethodInfo fn)
        {
            switch (fn.Name) {
                case "op_Implicit":
                    return null;
                case "op_Explicit":
                    return null;
                case "op_Addition":
                    return ReductionDeclarationForBinaryOperator(fn, "+");
                case "op_Subtraction":
                    return ReductionDeclarationForBinaryOperator(fn, "+");
                case "op_Multiply":
                    return ReductionDeclarationForBinaryOperator(fn, "*");
                case "op_Division":
                    return ReductionDeclarationForBinaryOperator(fn, "/");
                case "op_Modulus":
                    return ReductionDeclarationForBinaryOperator(fn, "%");
                case "op_ExclusiveOr":
                    return ReductionDeclarationForBinaryOperator(fn, "xor");
                case "op_BitwiseAnd":
                    return ReductionDeclarationForBinaryOperator(fn, "bitwise", "and");
                case "op_BitwiseOr":
                    return ReductionDeclarationForBinaryOperator(fn, "bitwise", "or");
                case "op_LogicalAnd":
                    return ReductionDeclarationForBinaryOperator(fn, "and");
                case "op_LogicalOr":
                    return ReductionDeclarationForBinaryOperator(fn, "or");
                case "op_LeftShift":
                    return ReductionDeclarationForBinaryOperator(fn, "<<");
                case "op_RightShift":
                    return ReductionDeclarationForBinaryOperator(fn, ">>");
                case "op_SignedRightShift":
                    return null;
                case "op_UnsignedRightShift":
                    return null;
                case "op_Equality":
                    return ReductionDeclarationForBinaryOperator(fn, "=");
                case "op_GreaterThan":
                    return ReductionDeclarationForBinaryOperator(fn, ">");
                case "op_LessThan":
                    return ReductionDeclarationForBinaryOperator(fn, "<");
                case "op_Inequality":
                    return ReductionDeclarationForBinaryOperator(fn, "!", "=");
                case "op_GreaterThanOrEqual":
                    return ReductionDeclarationForBinaryOperator(fn, ">", "=");
                case "op_LessThanOrEqual":
                    return ReductionDeclarationForBinaryOperator(fn, "<", "=");
                case "op_UnaryNegation":
                    return ReductionDeclarationForPrefixOperator(fn, "-");
                default:
                    throw new NotImplementedException();
            }
        }

        private static ReductionDeclaration ReductionDeclarationForBinaryOperator(MethodInfo fn, params Identifier[] op)
        {
            if (fn.IsGenericMethod) {
                return null;
            }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            var firstParam = fn.GetParameters().First();
            var firstTangentType = DotNetType.For(firstParam.ParameterType);
            if (firstTangentType == null) { return null; }

            var secondParam = fn.GetParameters().Skip(1).First();
            var secondTangentType = DotNetType.For(secondParam.ParameterType);
            if (secondTangentType == null) { return null; }

            var phrase = new PhrasePart[] { new PhrasePart(new ParameterDeclaration(firstParam.Name, firstTangentType)) }.Concat(op.Select(id => new PhrasePart(id))).Concat(new PhrasePart[] { new PhrasePart(new ParameterDeclaration(secondParam.Name, secondTangentType)) });
            return new ReductionDeclaration(phrase, new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, phrase.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), Enumerable.Empty<TangentType>()) }, Enumerable.Empty<ParameterDeclaration>())));
        }

        private static ReductionDeclaration ReductionDeclarationForPrefixOperator(MethodInfo fn, params Identifier[] op)
        {
            if (fn.IsGenericMethod) {
                return null;
            }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            var firstParam = fn.GetParameters().First();
            var firstTangentType = DotNetType.For(firstParam.ParameterType);
            if (firstTangentType == null) { return null; }

            var phrase = op.Select(id => new PhrasePart(id)).Concat(new PhrasePart[] { new PhrasePart(new ParameterDeclaration(firstParam.Name, firstTangentType)) });
            return new ReductionDeclaration(phrase, new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, phrase.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), Enumerable.Empty<TangentType>()) }, Enumerable.Empty<ParameterDeclaration>())));
        }
    }
}
