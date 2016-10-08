using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Errors;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public static class TypeResolve
    {
        public static ResultOrParseError<IEnumerable<TypeDeclaration>> AllPartialTypeDeclarations(IEnumerable<PartialTypeDeclaration> partialTypes, IEnumerable<TypeDeclaration> builtInTypes, out Dictionary<PartialParameterDeclaration, ParameterDeclaration> genericArgumentMapping)
        {
            List<TypeDeclaration> types = new List<TypeDeclaration>(builtInTypes);
            var simpleTypes = partialTypes.Where(ptd => ptd.Takes.All(pp => pp.IsIdentifier));
            types.AddRange(simpleTypes.Select(ptd => new TypeDeclaration(ptd.Takes.Select(ppp => new PhrasePart(ppp.Identifier.Identifier)), ptd.Returns)));
            var leftToProcess = partialTypes.Except(simpleTypes).ToList();
            genericArgumentMapping = new Dictionary<PartialParameterDeclaration, ParameterDeclaration>();

            while (leftToProcess.Any()) {
                List<PartialTypeDeclaration> removals = new List<PartialTypeDeclaration>();

                foreach (var entry in leftToProcess) {
                    var resolution = TryPartialTypeDeclaration(entry, types, false);
                    if (resolution != null) {
                        if (!resolution.Success) {
                            return new ResultOrParseError<IEnumerable<TypeDeclaration>>(resolution.Error);
                        }

                        removals.Add(entry);
                        types.Add(resolution.Result);
                        foreach (var genericMapping in entry.Takes.Where(ppp => !ppp.IsIdentifier).Select(ppp => ppp.Parameter).Zip(resolution.Result.Takes.Where(pp => !pp.IsIdentifier).Select(pp => pp.Parameter), (ppp, pp) => new KeyValuePair<PartialParameterDeclaration, ParameterDeclaration>(ppp, pp))) {
                            genericArgumentMapping.Add(genericMapping.Key, genericMapping.Value);
                        }
                    }
                }

                if (removals.Any()) {
                    leftToProcess = leftToProcess.Except(removals).ToList();
                } else {
                    return new ResultOrParseError<IEnumerable<TypeDeclaration>>(new AggregateParseError(leftToProcess.SelectMany(ptd => ptd.Takes.Where(ppp => !ppp.IsIdentifier).Select(ppp => new IncomprehensibleStatementError(ppp.Parameter.Returns)))));
                }
            }

            // Unfortunately, what we resolved mid-way into building types might not be the same thing we resolve now that we have all the types.
            // Even though it is costly, we will double check and toss if things no longer parse unambiguously.
            var genericTypes = partialTypes.Except(simpleTypes).Select(pt => Tuple.Create(pt, TryPartialTypeDeclaration(pt, types, true))).ToList();
            var issues = genericTypes.Where(i => !i.Item2.Success).Select(i => i.Item2).ToList();
            if (issues.Any()) {
                return new ResultOrParseError<IEnumerable<TypeDeclaration>>(new AggregateParseError(issues.Select(i => i.Error)));
            }

            return types;
        }

        public static ResultOrParseError<TypeDeclaration> TryPartialTypeDeclaration(PartialTypeDeclaration partial, IEnumerable<TypeDeclaration> types, bool hardError)
        {
            var scope = new TransformationScopeNew(new TransformationRule[] { LazyOperator.Common, SingleValueAccessor.Common }.Concat(types.Select(td => new TypeAccess(td))), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>()));
            List<PhrasePart> takes = new List<PhrasePart>();
            foreach (var t in partial.Takes) {
                if (t.IsIdentifier) {
                    takes.Add(new PhrasePart(t.Identifier.Identifier));
                } else {
                    var interpretResults = scope.InterpretTowards(TangentType.Any.Kind, t.Parameter.Returns);
                    if (interpretResults.Count == 1) {
                        if (!t.Parameter.Takes.All(ppp => ppp.IsIdentifier)) {
                            throw new NotImplementedException("Delegate parameter in type declaration not yet supported.");
                        }

                        takes.Add(new PhrasePart(new ParameterDeclaration(t.Parameter.Takes.Select(ppp => ppp.Identifier.Identifier), interpretResults.Cast<TypeAccessExpression>().First().TypeConstant.Value.Kind)));
                    } else if (interpretResults.Count == 0) {
                        // No way to parse things. 
                        if (!hardError) {
                            return null;
                        } else {
                            return new ResultOrParseError<TypeDeclaration>(new IncomprehensibleStatementError(t.Parameter.Returns));
                        }
                    } else {
                        return new ResultOrParseError<TypeDeclaration>(new AmbiguousStatementError(t.Parameter.Returns, interpretResults));
                    }
                }
            }

            return new TypeDeclaration(takes, partial.Returns);
        }

        public static ResultOrParseError<IEnumerable<ReductionDeclaration>> AllPartialFunctionDeclarations(IEnumerable<PartialReductionDeclaration> partialFunctions, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            var results = new List<ReductionDeclaration>();

            foreach (var fn in partialFunctions) {
                var resolutionResult = PartialFunctionDeclaration(fn, types, conversions);
                if (resolutionResult.Success) {
                    results.Add(resolutionResult.Result);
                } else {
                    errors = errors.Concat(resolutionResult.Error);
                }
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<IEnumerable<ReductionDeclaration>>(errors);
            }

            return results;
        }

        public static ResultOrParseError<IEnumerable<TypeDeclaration>> AllTypePlaceholders(IEnumerable<TypeDeclaration> typeDecls, Dictionary<PartialParameterDeclaration, ParameterDeclaration> genericArgumentMapping, List<InterfaceBinding> interfaceToImplementerBindings, List<PartialInterfaceBinding> standaloneInterfaceBindings, out Dictionary<TangentType, TangentType> placeholderConversions, out IEnumerable<ReductionDeclaration> additionalRules)
        {
            AggregateParseError errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            Dictionary<TangentType, TangentType> inNeedOfPopulation = new Dictionary<TangentType, TangentType>();
            List<Tuple<TangentType, TangentType>> bindings = new List<Tuple<TangentType, TangentType>>();
            List<ReductionDeclaration> delegateInvokers = new List<ReductionDeclaration>();
            Func<TangentType, TangentType> selector = t => t;
            selector = t => {
                if (t is PartialProductType) {
                    var newb = new ProductType(Enumerable.Empty<PhrasePart>(), Enumerable.Empty<ParameterDeclaration>(), Enumerable.Empty<Field>());
                    bindings.AddRange((t as PartialProductType).InterfaceReferences.Select(iface => new Tuple<TangentType, TangentType>(iface, newb)));
                    inNeedOfPopulation.Add((PartialProductType)t, newb);
                    return newb;
                } else if (t is PartialTypeReference) {
                    var reference = (PartialTypeReference)t;
                    var target = reference.ResolvedType == null ? reference : reference.ResolvedType;
                    if (!inNeedOfPopulation.ContainsKey(reference)) {
                        inNeedOfPopulation.Add(reference, target);
                    } else {
                        inNeedOfPopulation[reference] = target;
                    }

                    return target;
                } else if (t is PartialInterface) {
                    var tiff = t as PartialInterface;
                    var newb = new TypeClass(Enumerable.Empty<ReductionDeclaration>());
                    inNeedOfPopulation.Add((PartialInterface)t, newb);
                    return newb;
                } else {
                    return t;
                }
            };

            var newLookup = typeDecls.Select(td => new TypeDeclaration(td.Takes, selector(td.Returns))).ToList();
            var references = new HashSet<PartialTypeReference>();
            foreach (var interfaceDecl in standaloneInterfaceBindings) {
                foreach (var generic in interfaceDecl.TypePhrase.Where(ppp => !ppp.IsIdentifier)) {
                    // TODO: resolve these partial generics back to the generic in the real type during ResolveType.
                    throw new NotImplementedException("Sorry, standalone generic interface bindings don't currently work.");
                }

                var resolvedType = ResolveType(interfaceDecl.TypePhrase.Select(ppp => new IdentifierExpression(ppp.Identifier.Identifier, ppp.Identifier.SourceInfo)), newLookup, Enumerable.Empty<ParameterDeclaration>());
                if (resolvedType.Success) {
                    inNeedOfPopulation.Add(interfaceDecl, resolvedType.Result);
                    foreach (var referencedInterface in interfaceDecl.InterfaceReferences) {
                        if (referencedInterface is PartialTypeReference) {
                            var resolvedInterface = selector(referencedInterface);
                            bindings.Add(Tuple.Create(resolvedInterface, resolvedType.Result));
                        } else {
                            throw new ApplicationException("Shouldn't get here.");
                        }
                    }
                } else {
                    errors = errors.Concat(resolvedType.Error);
                }
            }


            foreach (var entry in inNeedOfPopulation) {
                if (entry.Key is PartialProductType) {
                    var ppt = (PartialProductType)entry.Key;
                    var resolvedType = PartialProductType(ppt, (ProductType)entry.Value, newLookup, ppt.GenericArguments.Select(ppd => genericArgumentMapping[ppd]));
                    if (!resolvedType.Success) {
                        errors = errors.Concat(resolvedType.Error);
                    } else {
                        delegateInvokers.AddRange(resolvedType.Result.Item2);
                    }
                } else if (entry.Key is PartialTypeReference) {
                    var reference = (PartialTypeReference)entry.Key;
                    references.Add(reference);
                    var resolvedType = ResolveType(reference.Identifiers, newLookup, reference.GenericArgumentPlaceholders.Select(ppd => genericArgumentMapping[ppd]));
                    if (resolvedType.Success) {
                        reference.ResolvedType = resolvedType.Result;
                    } else {
                        errors = errors.Concat(resolvedType.Error);
                    }
                } else if (entry.Key is PartialInterface) {
                    // Nothing, just need it in placeholder lists so that anything that found it earlier gets fixed.
                } else if (entry.Key is PartialInterfaceBinding) {
                    // Nothing. Just need the mapping here so that scopes get sent to the right location.
                } else {
                    throw new NotImplementedException();
                }
            }

            placeholderConversions = inNeedOfPopulation;
            additionalRules = delegateInvokers;

            if (errors.Errors.Any()) {
                return new ResultOrParseError<IEnumerable<TypeDeclaration>>(errors);
            }

            newLookup = newLookup.Select(td => new TypeDeclaration(td.Takes, selector(td.Returns))).ToList();

            var newBindings = new List<InterfaceBinding>(bindings.Count);
            foreach (var entry in bindings) {
                var iface = entry.Item1;
                if (entry.Item1 is PartialTypeReference) {
                    var reference = (PartialTypeReference)entry.Item1;
                    var resolvedType = ResolveType(reference.Identifiers, newLookup, reference.GenericArgumentPlaceholders.Select(ppd => genericArgumentMapping[ppd]));
                    if (resolvedType.Success) {
                        iface = resolvedType.Result;
                    } else {
                        errors = errors.Concat(resolvedType.Error);
                    }
                }

                // TODO: impl?
                newBindings.Add(new InterfaceBinding((TypeClass)iface, entry.Item2));
            }

            interfaceToImplementerBindings.AddRange(newBindings);

            foreach (var entry in newLookup) {
                var pt = entry.Returns as ProductType;
                if (pt != null && !pt.GenericParameters.Any()) {
                    var genericParameters = entry.Takes.Aggregate(new List<ParameterDeclaration>(), (pds, pp) => {
                        if (!pp.IsIdentifier) {
                            pds.Add(pp.Parameter);
                        }

                        return pds;
                    });

                    pt.GenericParameters.AddRange(genericParameters);
                }
            }

            return new ResultOrParseError<IEnumerable<TypeDeclaration>>(newLookup);
        }

        public static ResultOrParseError<IEnumerable<Field>> AllGlobalFields(IEnumerable<VarDeclElement> partialFields, IEnumerable<TypeDeclaration> types)
        {
            List<ParseError> errors = new List<ParseError>();
            List<Field> results = new List<Field>();
            foreach (var entry in partialFields) {
                var result = ResolveField(entry, types, null, Enumerable.Empty<ParameterDeclaration>());
                if (result.Success) {
                    results.Add(result.Result);
                } else {
                    errors.Add(result.Error);
                }
            }

            if (errors.Any()) {
                return new ResultOrParseError<IEnumerable<Field>>(new AggregateParseError(errors));
            }

            return results;
        }

        internal static ResultOrParseError<ReductionDeclaration> PartialFunctionDeclaration(PartialReductionDeclaration partialFunction, IEnumerable<TypeDeclaration> types, Dictionary<TangentType, TangentType> conversions)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            var phrase = new List<PhrasePart>();
            bool thisFound = false;

            TangentType scope = null;
            if (partialFunction.Returns.Scope != null) {
                scope = conversions[partialFunction.Returns.Scope];
            }

            var inferredTypes = ExtractAndCompileInferredTypes(partialFunction, types, scope == null ? Enumerable.Empty<ParameterDeclaration>() : scope == null ? Enumerable.Empty<ParameterDeclaration>() : scope.ContainedGenericReferences(GenericTie.Reference));
            if (!inferredTypes.Success) {
                return new ResultOrParseError<ReductionDeclaration>(inferredTypes.Error);
            }

            var genericFnParams = inferredTypes.Result.Select(kvp => kvp.Value.GenericArgument);

            foreach (var part in partialFunction.Takes) {
                if (!part.IsIdentifier && part.Parameter.IsThisParam) {

                    if (scope is TypeClass) {
                        var thisGeneric = (scope as TypeClass).ThisBindingInRequiredFunctions;
                        // first this, generic inference.
                        // Other thises, generic reference.
                        if (!thisFound) {
                            genericFnParams = genericFnParams.Concat(new[] { thisGeneric });
                            phrase.Add(new PhrasePart(new ParameterDeclaration("this", GenericInferencePlaceholder.For(thisGeneric))));
                        } else {
                            phrase.Add(new PhrasePart(new ParameterDeclaration("this", GenericArgumentReferenceType.For(thisGeneric))));
                        }
                    } else {
                        if (thisFound) { // TODO: nicer error.
                            throw new ApplicationException("Multiple this parameters declared in function.");
                        }

                        phrase.Add(new PhrasePart(new ParameterDeclaration("this", scope)));
                    }

                    thisFound = true;
                } else {
                    var resolved = Resolve(FixInferences(part, inferredTypes.Result), types);
                    if (resolved.Success) {
                        phrase.Add(resolved.Result);
                    } else {
                        errors = errors.Concat(resolved.Error);
                    }
                }
            }

            var fn = partialFunction.Returns;
            var effectiveType = ResolveType(fn.EffectiveType, types, genericFnParams);
            if (!effectiveType.Success) {
                errors = errors.Concat(effectiveType.Error);
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<ReductionDeclaration>(errors);
            }

            return new ResultOrParseError<ReductionDeclaration>(new ReductionDeclaration(phrase, new TypeResolvedFunction(effectiveType.Result, fn.Implementation, scope), genericFnParams));
        }

        internal static ResultOrParseError<Tuple<ProductType, IEnumerable<ReductionDeclaration>>> PartialProductType(PartialProductType partialType, ProductType target, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            var errors = new AggregateParseError(Enumerable.Empty<ParseError>());
            var delegateBindings = new List<ReductionDeclaration>();
            types = types.Concat(new[] { new TypeDeclaration("this", target) });

            foreach (var part in partialType.DataConstructorParts) {
                //throw new NotImplementedException("LASTWORKED: not handling PartialTypeInferenceBits here properly.");
                var resolved = Resolve(part, types, genericArguments);
                if (resolved.Success) {
                    target.DataConstructorParts.Add(resolved.Result);
                } else {
                    errors = errors.Concat(resolved.Error);
                }
            }

            foreach (var field in partialType.Fields) {
                var resolved = ResolveField(field, types, target, genericArguments);
                if (resolved.Success) {
                    target.Fields.Add(resolved.Result);
                } else {
                    errors = errors.Concat(resolved.Error);
                }
            }

            foreach (var delegateEntry in partialType.Delegates) {
                var fn = PartialFunctionDeclaration(new PartialReductionDeclaration(delegateEntry.FunctionPart, delegateEntry.DefaultImplementation), types, new Dictionary<TangentType, TangentType>() { { partialType, target } });
                if (!fn.Success) {
                    errors = errors.Concat(fn.Error);
                } else {
                    var delegateType = DelegateType.For(fn.Result.Takes.Where(pp => !pp.IsIdentifier && !pp.Parameter.IsThisParam).Select(pp => pp.Parameter.RequiredArgumentType), fn.Result.Returns.EffectiveType);
                    var fieldName = ResolveFieldName(delegateEntry.FieldPart, target);
                    if (!fieldName.Success) {
                        errors = errors.Concat(fieldName.Error);
                    } else {
                        var delegateField = new Field(new ParameterDeclaration(fieldName.Result, delegateType), new InitializerPlaceholder(new PartialStatement(new PartialElement[] {
                                        new LambdaElement(delegateEntry.FunctionPart.Where(ppp=>!ppp.IsIdentifier && !ppp.Parameter.IsThisParam).Select(ppp=>new VarDeclElement( ppp.Parameter/*new PartialParameterDeclaration( ppp.Parameter.Takes, null)*/, null, null)).ToList(), new BlockElement( delegateEntry.DefaultImplementation.Implementation)) })));

                        target.Fields.Add(delegateField);
                        delegateBindings.Add(
                            new ReductionDeclaration(
                                fn.Result.Takes,
                                new Function(
                                    fn.Result.Returns.EffectiveType,
                                    new Block(
                                        new Expression[] {
                                                        new DelegateInvocationExpression(
                                                            new FieldAccessorExpression(target, delegateField),
                                                            fn.Result.Takes.Where(pp=>!pp.IsIdentifier && !pp.Parameter.IsThisParam).Select(pp=>new ParameterAccessExpression(pp.Parameter, null)),
                                                            null)},
                                        Enumerable.Empty<ParameterDeclaration>()))));
                    }
                }
            }

            if (errors.Errors.Any()) {
                return new ResultOrParseError<Tuple<ProductType, IEnumerable<ReductionDeclaration>>>(errors);
            }

            return new Tuple<ProductType, IEnumerable<ReductionDeclaration>>(target, delegateBindings);
        }


        internal static ResultOrParseError<PhrasePart> Resolve(PartialPhrasePart partial, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> ctorGenericArguments = null)
        {
            if (partial.IsIdentifier) {
                return new PhrasePart(partial.Identifier.Identifier);
            }

            var resolved = Resolve(partial.Parameter, types, ctorGenericArguments);
            if (resolved.Success) {
                return new ResultOrParseError<PhrasePart>(new PhrasePart(resolved.Result));
            } else {
                return new ResultOrParseError<PhrasePart>(resolved.Error);
            }
        }

        internal static ResultOrParseError<ParameterDeclaration> Resolve(PartialParameterDeclaration partial, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> ctorGenericArguments = null)
        {
            var typeExprs = partial.Returns;
            if (ctorGenericArguments != null) {
                // We're resolving product type constructors. We need to fix any type inferences here.
                for (int ix = 0; ix < typeExprs.Count; ++ix) {
                    var inference = typeExprs[ix] as PartialTypeInferenceExpression;
                    if (inference != null) {
                        // First, check if it's a reference to the type generic.
                        var match = ctorGenericArguments.FirstOrDefault(pd => {
                            var genericName = pd.Takes.Select(pp => pp.Identifier).ToList();
                            return inference.InferenceName.Count() == genericName.Count && inference.InferenceName.SequenceEqual(genericName);
                        });

                        if (match != null) {
                            typeExprs[ix] = new GenericInferenceParameterAccessExpression(GenericInferencePlaceholder.For(match), typeExprs[ix].SourceInfo);
                        } else {
                            // otherwise, we're doing type inference in the constructor, but _not_ using it for the type's generic params.
                            throw new NotImplementedException("Sorry, generic inference in constructors must infer a generic parameter for the generic type at this time.");
                        }
                    }
                }
            }

            var type = ResolveType(typeExprs, types, ctorGenericArguments ?? Enumerable.Empty<ParameterDeclaration>());
            if (!type.Success) {
                return new ResultOrParseError<ParameterDeclaration>(type.Error);
            }

            var takeResolutions = new List<PhrasePart>();
            foreach (var ppp in partial.Takes) {
                if (ppp.IsIdentifier) {
                    takeResolutions.Add(new PhrasePart(ppp.Identifier.Identifier));
                } else {
                    if (!ppp.Parameter.Takes.All(inner => inner.IsIdentifier)) {
                        throw new NotImplementedException("Nested higher level function parameters not yet supported.");
                    }

                    var partialResult = ResolveType(ppp.Parameter.Takes.Select(inner => inner.Identifier), types, ctorGenericArguments ?? Enumerable.Empty<ParameterDeclaration>());
                    if (partialResult.Success) {
                        takeResolutions.Add(new PhrasePart(new ParameterDeclaration("_", partialResult.Result)));
                    } else {
                        return new ResultOrParseError<ParameterDeclaration>(partialResult.Error);
                    }
                }
            }

            return new ParameterDeclaration(takeResolutions, ctorGenericArguments == null ? type.Result : ConvertGenericReferencesToInferences(type.Result));
        }

        private static ResultOrParseError<GenericInferencePlaceholder> Resolve(PartialTypeInferenceExpression inference, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> ctorGenericArguments = null)
        {

            var type = ResolveType(inference.InferenceExpression, types, ctorGenericArguments ?? Enumerable.Empty<ParameterDeclaration>());
            if (!type.Success) {
                return new ResultOrParseError<GenericInferencePlaceholder>(type.Error);
            }

            return GenericInferencePlaceholder.For(new ParameterDeclaration(inference.InferenceName, type.Result.Kind));
        }

        internal static ResultOrParseError<TangentType> ResolveType(IEnumerable<Expression> identifiers, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            var scope = new TransformationScopeNew(new TransformationRule[] { LazyOperator.Common, SingleValueAccessor.Common }.Concat(types.Select(td => (TransformationRule)new TypeAccess(td))).Concat(genericArguments.Select(ga => new GenericParameterAccess(ga))), new ConversionGraph(Enumerable.Empty<ReductionDeclaration>()));
            var result = scope.InterpretTowards(TangentType.Any.Kind, identifiers.ToList());
            if (result.Count == 1) {
                var resolvedType = result[0].EffectiveType;

                if (resolvedType.ImplementationType == KindOfType.TypeConstant) {
                    resolvedType = ((TypeConstant)resolvedType).Value;

                    var reference = resolvedType as PartialTypeReference;
                    if (reference != null) {
                        return ResolvePlaceholderReference(reference, types, genericArguments);
                    }

                    return resolvedType;
                } else if (resolvedType.ImplementationType == KindOfType.GenericReference || resolvedType.ImplementationType == KindOfType.InferencePoint) {
                    // some type reference. Just go with it?
                    return resolvedType;
                } else {
                    throw new NotImplementedException();
                }
            }

            if (result.Count == 0) {
                return new ResultOrParseError<TangentType>(new IncomprehensibleStatementError(identifiers));
            } else {
                return new ResultOrParseError<TangentType>(new AmbiguousStatementError(identifiers, result));
            }
        }

        private static ResultOrParseError<TangentType> ResolvePlaceholderReference(PartialTypeReference reference, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> genericArguments)
        {
            if (reference.ResolvedType == null) {
                var nested = ResolveType(reference.Identifiers, types, genericArguments);
                if (nested.Success) {
                    reference.ResolvedType = nested.Result;
                } else {
                    return new ResultOrParseError<TangentType>(nested.Error);
                }
            }

            return reference.ResolvedType;
        }

        private static ResultOrParseError<Field> ResolveField(VarDeclElement field, IEnumerable<TypeDeclaration> types, ProductType target, IEnumerable<ParameterDeclaration> genericArguments)
        {
            var fieldName = ResolveFieldName(field.ParameterDeclaration.Takes, target);
            if (!fieldName.Success) {
                return new ResultOrParseError<Field>(fieldName.Error);
            }

            var fieldType = ResolveType(field.ParameterDeclaration.Returns, types, genericArguments);
            if (!fieldType.Success) {
                return new ResultOrParseError<Field>(fieldType.Error);
            }

            return new Field(new ParameterDeclaration(fieldName.Result, fieldType.Result), new InitializerPlaceholder(field.Initializer));
        }

        private static ResultOrParseError<IEnumerable<PhrasePart>> ResolveFieldName(IEnumerable<PartialPhrasePart> name, ProductType target)
        {
            var parameters = name.Where(ppp => !ppp.IsIdentifier).ToList();
            if (target == null) {
                if (parameters.Count != 0) {
                    return new ResultOrParseError<IEnumerable<PhrasePart>>(new FieldWithTooManyThisError());
                }
            } else {
                if (parameters.Count == 0) {
                    return new ResultOrParseError<IEnumerable<PhrasePart>>(new FieldWithoutThisError());
                }

                if (parameters.Count > 1) {
                    return new ResultOrParseError<IEnumerable<PhrasePart>>(new FieldWithTooManyThisError());
                }

                if (parameters.Count == name.Count()) {
                    return new ResultOrParseError<IEnumerable<PhrasePart>>(new FieldWithoutIdentifiersError());
                }
            }

            return new ResultOrParseError<IEnumerable<PhrasePart>>(name.Select(ppp => ppp.IsIdentifier ? new PhrasePart(ppp.Identifier.Identifier) : new PhrasePart(new ParameterDeclaration("this", target))));
        }

        private static TangentType ConvertGenericReferencesToInferences(TangentType input)
        {
            switch (input.ImplementationType) {
                case KindOfType.BoundGeneric:
                    var boundGeneric = input as BoundGenericType;
                    return BoundGenericType.For(boundGeneric.GenericTypeDeclatation, boundGeneric.TypeArguments.Select(ta => ConvertGenericReferencesToInferences(ta)));
                case KindOfType.Builtin:
                case KindOfType.Enum:
                case KindOfType.SingleValue:
                case KindOfType.InferencePoint:
                case KindOfType.Placeholder:
                case KindOfType.Product:
                    return input;
                case KindOfType.GenericReference:
                    var genref = input as GenericArgumentReferenceType;
                    return GenericInferencePlaceholder.For(genref.GenericParameter);
                case KindOfType.Kind:
                    var kind = input as KindType;
                    return ConvertGenericReferencesToInferences(kind.KindOf).Kind;
                default:
                    throw new NotImplementedException();
            }
        }

        private static ResultOrParseError<Dictionary<PartialTypeInferenceExpression, GenericInferencePlaceholder>> ExtractAndCompileInferredTypes(PartialReductionDeclaration partialFunctionDecl, IEnumerable<TypeDeclaration> types, IEnumerable<ParameterDeclaration> typeGenerics)
        {
            var inferenceParameters = partialFunctionDecl.Takes.Where(ppp => !ppp.IsIdentifier).SelectMany(ppp => ppp.Parameter.Returns.Where(expr => expr.NodeType == ExpressionNodeType.TypeInference)).Cast<PartialTypeInferenceExpression>().ToList();
            var dependencyGraph = new Dictionary<PartialTypeInferenceExpression, HashSet<PartialTypeInferenceExpression>>();
            foreach (var tie in inferenceParameters) {
                BuildInferenceDependencyGraph(tie, dependencyGraph);
            }

            var result = new Dictionary<PartialTypeInferenceExpression, GenericInferencePlaceholder>();
            while (result.Count < dependencyGraph.Count) {
                var workset = dependencyGraph.Where(kvp => !result.ContainsKey(kvp.Key) && kvp.Value.All(tie => result.ContainsKey(tie))).Select(kvp => kvp.Key).ToList();
                foreach (var entry in workset) {
                    var resolved = Resolve(entry, types, typeGenerics);
                    if (!resolved.Success) {
                        return new ResultOrParseError<Dictionary<PartialTypeInferenceExpression, GenericInferencePlaceholder>>(resolved.Error);
                    }

                    result.Add(entry, resolved.Result);
                }
            }

            return result;
        }

        private static void BuildInferenceDependencyGraph(PartialTypeInferenceExpression tie, Dictionary<PartialTypeInferenceExpression, HashSet<PartialTypeInferenceExpression>> graph)
        {
            if (graph.ContainsKey(tie)) { return; }
            graph.Add(tie, new HashSet<PartialTypeInferenceExpression>());
            foreach (var dep in tie.InferenceExpression.Where(expr => expr.NodeType == ExpressionNodeType.TypeInference).Cast<PartialTypeInferenceExpression>()) {
                BuildInferenceDependencyGraph(dep, graph);
                graph[tie].Add(dep);
            }
        }

        private static PartialPhrasePart FixInferences(PartialPhrasePart part, Dictionary<PartialTypeInferenceExpression, GenericInferencePlaceholder> inferredTypes)
        {
            if (part.IsIdentifier) { return part; }
            Func<Expression, Expression> fixer = expr => {
                var partial = expr as PartialTypeInferenceExpression;
                if (partial == null) {
                    return expr;
                }

                return new GenericInferenceParameterAccessExpression(inferredTypes[partial], partial.SourceInfo);
            };

            return new PartialPhrasePart(new PartialParameterDeclaration(part.Parameter.Takes, part.Parameter.Returns.Select(fixer).ToList()));
        }
    }
}
