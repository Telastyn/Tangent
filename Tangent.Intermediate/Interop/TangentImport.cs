using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        private static readonly Type GenericConstantExpressionType = typeof(ConstantExpression<>);

        public static ImportBundle ImportAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Aggregate(new PartialImportBundle(), (r, a) => PartialImportBundle.Merge(r, ImportAssembly(a)));
        }

        public static PartialImportBundle ImportAssembly(Assembly assembly)
        {
            PartialImportBundle result = new PartialImportBundle();

            foreach (var t in assembly.GetTypes()) {
                if (t.IsPublic) {
                    var typeDecl = DotNetType.TypeDeclarationFor(t);
                    if (typeDecl != null) {
                        var tangentType = typeDecl.Returns;
                        if (t != typeof(int) && t != typeof(string) && t != typeof(bool) && t != typeof(double) && t != typeof(void)) {
                            result.Types.Add(t, typeDecl);
                        }

                        foreach (var fn in t.GetMethods()) {
                            if (fn.IsPublic) {
                                var tfn = ReductionDeclarationFor(typeDecl, fn);
                                if (tfn != null) {
                                    result.CommonFunctions.Add(fn, tfn);
                                }
                            }
                        }

                        foreach (var property in t.GetProperties()) {
                            var propFns = ReductionDeclarationsFor(typeDecl, property);
                            if (propFns != null) {
                                foreach (var fn in propFns) {
                                    result.CommonFunctions.Add(fn.Takes.Any(pp => pp.IsIdentifier && pp.Identifier.Value == "=") ? property.GetSetMethod() : property.GetGetMethod(), fn);
                                }
                            }
                        }

                        foreach (var field in t.GetFields()) {
                            if (field.IsPublic) {
                                var fieldFns = ReductionDeclarationsFor(typeDecl, field);
                                if (fieldFns != null) {
                                    foreach (var fn in fieldFns) {
                                        if (fn.Takes.Any(pp => pp.IsIdentifier && pp.Identifier.Value == "=")) {
                                            result.FieldMutators.Add(field, fn);
                                        } else {
                                            result.FieldAccessors.Add(field, fn);
                                        }
                                    }
                                }

                            }
                        }

                        foreach (var ctor in t.GetConstructors()) {
                            if (ctor.IsPublic) {
                                var ctorFn = ReductionDeclarationFor(typeDecl, ctor);
                                if (ctorFn != null) {
                                    result.Constructors.Add(ctor, ctorFn);
                                }
                            }
                        }


                        if (t.IsValueType) {
                            result.StructInits.Add(t, ReductionDeclarationForStructInit(t));
                        }

                        result.SubtypingConversions.Add(t, new List<ReductionDeclaration>(SubTypingConversionsFor(t)));
                    }
                }
            }

            return result;
        }

        public static ReductionDeclaration ReductionDeclarationFor(TypeDeclaration typeDecl, MethodInfo fn)
        {
            if (fn.IsStatic) {
                return ReductionDeclarationForStatic(typeDecl, fn);
            }

            if (fn.IsGenericMethod) {
                // TODO:
                return null;
            }

            if (fn.IsSpecialName) {
                // A property? Event? Ctor? Punt for now.
                return null;
            }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
            var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
            var thisPart = new List<PhrasePart>() { new PhrasePart(new ParameterDeclaration("this", genericArguments.Any() ? BoundGenericType.For(typeDecl, genericArguments.Select(ga => GenericInferencePlaceholder.For(ga)).ToList()) : typeDecl.Returns)) };
            var methodPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(fn.Name) };
            var parameterPart = new List<PhrasePart>();

            foreach (var parameter in fn.GetParameters()) {
                if (parameter.IsOut) {
                    return null;
                }

                var parameterType = DotNetType.For(parameter.ParameterType);
                if (parameterType == null) { return null; }
                parameterPart.Add(new PhrasePart(new ParameterDeclaration(parameter.Name, parameterType)));
            }

            return new ReductionDeclaration(thisPart.Concat(methodPart).Concat(parameterPart), new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, thisPart.Concat(parameterPart).Select(pp=>pp.Parameter).ToList(), genericArguments.Select(ga=>GenericArgumentReferenceType.For(ga)).ToList()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments);
        }

        public static IEnumerable<ReductionDeclaration> ReductionDeclarationsFor(TypeDeclaration typeDecl, PropertyInfo property)
        {
            var propertyType = DotNetType.For(property.PropertyType);
            if (propertyType == null) {
                return null;
            }

            List<ReductionDeclaration> result = new List<ReductionDeclaration>();
            if (property.CanRead) {
                var readFn = property.GetGetMethod();
                if (readFn.IsPublic) {
                    if (readFn.GetParameters().Any()) { return null; }
                    if (readFn.IsStatic) {
                        
                        var namePart = typeDecl.Takes.TakeWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").ToList();
                        var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                        var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                        var propertyPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(property.Name)};
                        result.Add(new ReductionDeclaration(namePart.Concat(genericPart).Concat(propertyPart), new Function(propertyType, new Block(new Expression[] { new DirectCallExpression(readFn, propertyType, Enumerable.Empty<Expression>(), genericArguments.Select(ga => GenericArgumentReferenceType.For(ga)).ToList()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                    } else {
                        var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                        var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                        var thisPart = new List<PhrasePart>() { new PhrasePart(new ParameterDeclaration("this", genericArguments.Any() ? BoundGenericType.For(typeDecl, genericArguments.Select(ga => GenericInferencePlaceholder.For(ga))) : typeDecl.Returns)) };
                        var propertyPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(property.Name) };
                        result.Add(new ReductionDeclaration(thisPart.Concat(propertyPart), new Function(propertyType, new Block(new Expression[] { new DirectCallExpression(readFn, propertyType, new Expression[] { new ParameterAccessExpression(thisPart[0].Parameter, null) }, genericArguments.Select(ga => GenericArgumentReferenceType.For(ga)).ToList()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                    }
                }
            }

            if (property.CanWrite) {
                var writeFn = property.GetSetMethod();
                if (writeFn != null && writeFn.IsPublic) {
                    if(writeFn.GetParameters().Count() > 1) { return null; }
                    if (writeFn.IsStatic) {
                        var namePart = typeDecl.Takes.TakeWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").ToList();
                        var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                        var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                        var propertyPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(property.Name) };
                        var assignmentPart = new List<PhrasePart>() { new PhrasePart("="), new PhrasePart(new ParameterDeclaration("value", propertyType)) };
                        result.Add(new ReductionDeclaration(namePart.Concat(genericPart).Concat(propertyPart).Concat(assignmentPart), new Function(TangentType.Void, new Block(new Expression[] { new DirectCallExpression(writeFn, TangentType.Void, new Expression[] { new ParameterAccessExpression(assignmentPart[1].Parameter, null) }, genericArguments.Select(ga => GenericArgumentReferenceType.For(ga)).ToList()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                    } else {
                        var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                        var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                        var thisPart = new List<PhrasePart>() { new PhrasePart(new ParameterDeclaration("this", genericArguments.Any() ? BoundGenericType.For(typeDecl, genericArguments.Select(ga => GenericInferencePlaceholder.For(ga))) : typeDecl.Returns)) };
                        var propertyPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(property.Name) };
                        var assignmentPart = new List<PhrasePart>() { new PhrasePart("="), new PhrasePart(new ParameterDeclaration("value", propertyType)) };
                        result.Add(new ReductionDeclaration(thisPart.Concat(propertyPart).Concat(assignmentPart), new Function(TangentType.Void, new Block(new Expression[] { new DirectCallExpression(writeFn, TangentType.Void, new Expression[] { new ParameterAccessExpression(thisPart[0].Parameter, null), new ParameterAccessExpression(assignmentPart[1].Parameter, null) }, genericArguments.Select(ga => GenericArgumentReferenceType.For(ga)).ToList()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                    }
                }
            }

            return result;
        }

        public static IEnumerable<ReductionDeclaration> ReductionDeclarationsFor(TypeDeclaration typeDecl, FieldInfo field)
        {
            if (!field.IsPublic) { return null; }
            var propertyType = DotNetType.For(field.FieldType);
            if (propertyType == null) {
                return null;
            }

            List<ReductionDeclaration> result = new List<ReductionDeclaration>();

            var fieldPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(field.Name) };

            if (field.IsStatic) {
                var namePart = typeDecl.Takes.TakeWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").ToList();
                var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                
                result.Add(new ReductionDeclaration(namePart.Concat(genericPart).Concat(fieldPart), new Function(propertyType, new Block(new Expression[] { new DirectFieldAccessExpression(field, Enumerable.Empty<Expression>()) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                if (!(field.IsInitOnly || field.IsLiteral)) {
                    var assignmentPart = new List<PhrasePart>() { new PhrasePart("="), new PhrasePart(new ParameterDeclaration("value", propertyType)) };
                    result.Add(new ReductionDeclaration(namePart.Concat(genericPart).Concat(fieldPart).Concat(assignmentPart), new Function(TangentType.Void, new Block(new Expression[] { new DirectFieldAssignmentExpression(field, new Expression[] { new ParameterAccessExpression(assignmentPart[1].Parameter, null) }) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                }
            } else {
                var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                var thisPart = new List<PhrasePart>() { new ParameterDeclaration("this", genericArguments.Any() ? BoundGenericType.For(typeDecl, genericArguments.Select(ga => GenericInferencePlaceholder.For(ga)).ToList()) : typeDecl.Returns) };
                result.Add(new ReductionDeclaration(thisPart.Concat(fieldPart), new Function(propertyType, new Block(new Expression[] { new DirectFieldAccessExpression(field, new Expression[] { new ParameterAccessExpression(thisPart[0].Parameter, null) }) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                if (!(field.IsInitOnly || field.IsLiteral)) {
                    var assignmentPart = new List<PhrasePart>() { new PhrasePart("="), new PhrasePart(new ParameterDeclaration("value", propertyType)) };
                    result.Add(new ReductionDeclaration(thisPart.Concat(fieldPart).Concat(assignmentPart), new Function(TangentType.Void, new Block(new Expression[] { new DirectFieldAssignmentExpression(field, new Expression[] { new ParameterAccessExpression(thisPart[0].Parameter, null), new ParameterAccessExpression(assignmentPart[1].Parameter, null) }) }, Enumerable.Empty<ParameterDeclaration>())), genericArguments));
                }
            }

            return result;
        }

        public static ReductionDeclaration ReductionDeclarationForStatic(TypeDeclaration typeDecl, MethodInfo fn)
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

            var parameters = fn.GetParameters();

            var owningTangentType = DotNetType.For(fn.DeclaringType);
            if (owningTangentType == null) { return null; }

            var returnTangentType = DotNetType.For(fn.ReturnType);
            if (returnTangentType == null) { return null; }

            if (fn.DeclaringType.GetCustomAttribute<ExtensionAttribute>() != null &&
                fn.GetCustomAttribute<ExtensionAttribute>() != null) {
                // Some extension method. Bleh.

                var extensionType = DotNetType.For(parameters.First().ParameterType);
                if (extensionType == null) { return null; }
                if (parameters.First().ParameterType.IsGenericTypeDefinition) { return null; }

                var phrase = new List<PhrasePart>();
                phrase.Add(new PhrasePart(new ParameterDeclaration("this", extensionType)));
                phrase.Add(new PhrasePart("."));
                phrase.Add(new PhrasePart(fn.Name));

                foreach (var parameter in parameters.Skip(1)) {
                    if (parameter.IsOut) {
                        return null;
                    }

                    var parameterType = DotNetType.For(parameter.ParameterType);
                    if (parameterType == null) { return null; }
                    phrase.Add(new PhrasePart(new ParameterDeclaration(parameter.Name, parameterType)));
                }

                return new ReductionDeclaration(phrase, new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, phrase.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), Enumerable.Empty<TangentType>()) }, Enumerable.Empty<ParameterDeclaration>())));
            } else {

                List<PhrasePart> namePart = new List<PhrasePart>();
                if (fn.DeclaringType == typeof(int)) {
                    namePart.Add(new PhrasePart("int"));
                } else {
                    namePart = typeDecl.Takes.TakeWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").ToList();
                }

                var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
                var genericArguments = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
                var methodPart = new List<PhrasePart>() { new PhrasePart("."), new PhrasePart(fn.Name) };
                var parameterPart = new List<PhrasePart>();
                foreach (var parameter in parameters) {
                    if (parameter.IsOut) {
                        return null;
                    }

                    var parameterType = DotNetType.For(parameter.ParameterType);
                    if (parameterType == null) { return null; }
                    parameterPart.Add(new PhrasePart(new ParameterDeclaration(parameter.Name, parameterType)));
                }

                return new ReductionDeclaration(namePart.Concat(genericPart).Concat(methodPart).Concat(parameterPart), new Function(returnTangentType, new Block(new Expression[] { new DirectCallExpression(fn, returnTangentType, parameterPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), genericArguments.Select(ga=>GenericArgumentReferenceType.For(ga)).ToList()) }, genericArguments)));
            }
        }

        public static ReductionDeclaration ReductionDeclarationFor(TypeDeclaration typeDecl, ConstructorInfo ctor)
        {
            if (!ctor.IsPublic) {
                return null;
            }

            if (ctor.IsStatic) {
                return null;
            }

            var namePart = typeDecl.Takes.TakeWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").ToList();
            var genericPart = typeDecl.Takes.SkipWhile(pp => pp.IsIdentifier && pp.Identifier.Value != "<").Select(pp => pp.IsIdentifier ? pp : new PhrasePart(new ParameterDeclaration(pp.Parameter.Takes, pp.Parameter.Returns))).ToList();
            var parameterPart = ctor.GetParameters().Select(pi => {
                var pt = DotNetType.For(pi.ParameterType);
                if (pt == null) {
                    return null;
                }

                return new ParameterDeclaration(pi.Name, pt);
            });

            if (parameterPart.Any(pd => pd == null)) {
                return null;
            }

            var genericArgs = genericPart.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter).ToList();
            var targetType = genericArgs.Any() ? BoundGenericType.For(typeDecl, genericArgs.Select(ga => GenericArgumentReferenceType.For(ga))) : typeDecl.Returns;
            return new ReductionDeclaration(new[] { new PhrasePart("new") }.Concat(namePart).Concat(genericPart).Concat(parameterPart.Select(pd => new PhrasePart(pd))), new Function(targetType, new Block(new Expression[] { new DirectConstructorCallExpression(ctor, parameterPart.Select(pd => new ParameterAccessExpression(pd, null))) }, genericArgs)));
        }

        public static ReductionDeclaration ReductionDeclarationForStructInit(Type t)
        {
            if (!t.IsValueType) {
                return null;
            }

            var tangentType = DotNetType.For(t);
            if (tangentType == null) {
                return null;
            }

            var namePart = Tokenize.ProgramFile("new .NET " + t.FullName, "").Select(token => new PhrasePart(new Identifier(token.Value))).ToList();

            return new ReductionDeclaration(namePart, new Function(tangentType, new Block(new Expression[] { new DirectStructInitExpression(t) }, Enumerable.Empty<ParameterDeclaration>())));
        }

        public static IEnumerable<ReductionDeclaration> SubTypingConversionsFor(Type t)
        {
            var tangentType = DotNetType.For(t);

            // Base 
            if (t.IsValueType) {
                // T -> object
                // T -> object?
            } else {
                // T -> B
                // T? -> B?
                // null -> B?
                // T -> B?
            }

            // Interfaces
            // T -> I
            // T? -> I?
            // null -> I?
            // T -> I?

            if (!t.IsValueType) {
                // TODO: handle null for generics.
                if (!t.IsGenericTypeDefinition) {
                    var concreteConstantExpression = Activator.CreateInstance(GenericConstantExpressionType.MakeGenericType(new[] { t }), tangentType, null, null) as ConstantExpression;
                    yield return new ReductionDeclaration("null", new Function(tangentType, new Block(new Expression[] { concreteConstantExpression }, Enumerable.Empty<ParameterDeclaration>())));
                }
            }

            // TODO:
            yield break;
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
