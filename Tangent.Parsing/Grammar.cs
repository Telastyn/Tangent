﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing
{
    public static class Grammar
    {
        public static readonly Parser<IdentifierExpression> ID = IdentifierParser.Common;
        private static readonly Parser<string> Pipe = new StringLiteralParser("|");
        private static readonly Parser<string> Comma = new StringLiteralParser(",");
        private static readonly Parser<IdentifierExpression> LazyOperator = LiteralParser.LazyOperator.Select(x => new IdentifierExpression("~>", null));
        private static readonly Parser<string> Interface = new StringLiteralParser("interface");
        public static readonly Parser<ConstantElement<string>> StringConstant = new StringConstantParser();
        public static readonly Parser<ConstantElement<int>> IntConstant = new IntConstantParser();

        // (id+(:id+)?)
        public static readonly Parser<PartialParameterDeclaration> TypeDeclParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                ID.OneOrMore,
                Parser.Combine(LiteralParser.Colon, ID.OneOrMore, (c, typeref) => typeref).Maybe,
                LiteralParser.CloseParen,
                (o, phrase, typeref, c) => new PartialParameterDeclaration(phrase, (typeref ?? new List<IdentifierExpression>() { new IdentifierExpression("any", null) }).Select(idExpr => (Expression)idExpr).ToList()));

        // enum { id+, id+, ... }
        public static readonly Parser<TangentType> EnumImpl =
            Parser.Combine(
                new StringLiteralParser("enum"),
                LiteralParser.OpenCurly,
                Parser.Delimited(Comma, Parser.Difference(ID, Comma).OneOrMore, requiresOne: false, optionalTrailingDelimiter: false),
                LiteralParser.CloseCurly,
                (e, o, enums, c) => (TangentType)new EnumType(enums.Select(entry => entry.First().Identifier)));

        // (id|lazy)+
        public static readonly Parser<IEnumerable<IdentifierExpression>> TypeExpr = ID.Or(LazyOperator, "Identifier").OneOrMore;

        // (type-expr)
        public static readonly Parser<PartialPhrasePart> ParamParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                TypeExpr,
                LiteralParser.CloseParen,
                (o, expr, c) => new PartialPhrasePart(new PartialParameterDeclaration(expr.ToList(), new List<Expression>() { new IdentifierExpression("any", null) })));

        // (id|param-param)+
        public static readonly Parser<IEnumerable<PartialPhrasePart>> ParamNamePart = ID.Select(id => new PartialPhrasePart(id)).Or(ParamParam, "Parameter name part").OneOrMore;

        // (id+)
        public static readonly Parser<Expression> ParamInferencePlaceholder =
            Parser.Combine(
                LiteralParser.OpenParen,
                ID.OneOrMore,
                Parser.Combine(LiteralParser.Colon, TypeExpr, (c, constraint) => constraint).Maybe,
                LiteralParser.CloseParen,
                (o, ids, constraint, c) => (Expression)new PartialTypeInferenceExpression(ids.Select(id => id.Identifier), constraint ?? (IEnumerable<Expression>)new Expression[] { new IdentifierExpression("any", null) }, LineColumnRange.CombineAll(ids.Select(id => id.SourceInfo))));

        // (id|lazy|param-inference-placeholder)+
        public static readonly Parser<IEnumerable<Expression>> SimpleParamType =
            Parser.Options("Parameter type part",
                ID.Select(id => (Expression)id),
                LazyOperator.Select(id => (Expression)id),
                ParamInferencePlaceholder).OneOrMore;

        // id+:type-expr
        public static readonly Parser<IEnumerable<Expression>> ConstrainedGenericParamType =
            Parser.Combine(
                ID.OneOrMore,
                LiteralParser.Colon,
                TypeExpr,
                (generic, c, constraint) => (IEnumerable<Expression>)new Expression[] { new PartialTypeInferenceExpression(generic.Select(g => g.Identifier), constraint, LineColumnRange.CombineAll(generic.Select(g => g.SourceInfo))) });

        public static readonly Parser<IEnumerable<Expression>> ParamType =
            Parser.Options("Parameter type",
                ConstrainedGenericParamType,
                SimpleParamType);

        // ( name : type )
        public static readonly Parser<PartialPhrasePart> ParamDecl =
            Parser.Combine(
                LiteralParser.OpenParen,
                ParamNamePart,
                LiteralParser.Colon,
                ParamType,
                LiteralParser.CloseParen,
                (o, name, c, type, e) => new PartialPhrasePart(new PartialParameterDeclaration(name, type.ToList())));

        // (this)
        public static readonly Parser<PartialPhrasePart> thisParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                new StringLiteralParser("this"),
                LiteralParser.CloseParen,
                (o, t, c) => new PartialPhrasePart(new PartialParameterDeclaration(new IdentifierExpression("this", null), new List<Expression>() { new IdentifierExpression("this", null) })));

        // (id|param-decl)+
        public static readonly Parser<IEnumerable<PartialPhrasePart>> FunctionPhrase =
            Parser.Options("Phrase part",
                ID.Select(id => new PartialPhrasePart(id)),
                thisParam,
                ParamDecl
            ).OneOrMore;

        public static readonly Parser<IEnumerable<PartialPhrasePart>> FunctionPhraseSansPipe =
            Parser.Options("Phrase part",
                Parser.Difference(ID.Select(id => new PartialPhrasePart(id)), Pipe),
                thisParam,
                ParamDecl
            ).OneOrMore;

        // :< id+
        public static readonly Parser<TangentType> InlineInterfaceBinding =
            Parser.Combine(
                LiteralParser.InterfaceBindingOperator,
                ID.OneOrMore,
                (op, ids) => (TangentType)new PartialTypeReference(ids, Enumerable.Empty<PartialParameterDeclaration>()));

        public static readonly Parser<PartialElement> LambdaExpr =
            Parser.Combine(
                Parser.Options("Lambda parameters",
                    ID.Select(id => (IEnumerable<VarDeclElement>)new[] { new VarDeclElement(new PartialParameterDeclaration(id, null), null, id.SourceInfo) }),
                    Parser.Combine(LiteralParser.OpenParen, ID.OneOrMore, LiteralParser.CloseParen, (o, ids, c) => new VarDeclElement(new PartialParameterDeclaration(ids, null), null, LineColumnRange.CombineAll(ids.Select(id => id.SourceInfo)))).OneOrMore
                // TODO: full param decl?
                ),
                LiteralParser.FunctionArrow,
                Parser.Delegate(() => BlockDecl),
                (parameters, a, block) => (PartialElement)new LambdaElement(parameters.ToList(), new BlockElement(block)));

        public static readonly Parser<PartialElement> Expr =
            Parser.Options("Expression",
                LambdaExpr,
                ID.Select(id => (PartialElement)new IdentifierElement(id.Identifier, id.SourceInfo)),
                StringConstant.Select(x => (PartialElement)x),
                IntConstant.Select(x => (PartialElement)x),
                Parser.Delegate(() => ParenExpr),
                Parser.Delegate(() => BlockDecl.Select(block => (PartialElement)new BlockElement(block))));

        public static readonly Parser<PartialElement> ParenExpr =
            Parser.Combine(
                LiteralParser.OpenParen,
                Expr.OneOrMore,
                LiteralParser.CloseParen,
                (o, exprs, c) => (PartialElement)new BlockElement(new PartialBlock(new[] { new PartialStatement(exprs) }, Enumerable.Empty<VarDeclElement>())));

        public static readonly Parser<PartialStatement> Statement =
                Expr.OneOrMore.Select(
                    (exprs) => new PartialStatement(exprs));

        public static readonly Parser<IEnumerable<Expression>> SimpleTypeReference =
            ID.Select(id => (Expression)id).Or(LazyOperator.Select(id => (Expression)id), "type reference").OneOrMore;

        public static readonly Parser<VarDeclElement> LocalVar =
            Parser.Combine(
                LiteralParser.Colon,
                ID.OneOrMore,
                LiteralParser.Colon,
                SimpleTypeReference,
                LiteralParser.InitializerEquals,
                Statement,
                (c1, name, c2, typeref, ie, initializer) => new VarDeclElement(new PartialParameterDeclaration(name, typeref.ToList()), initializer, LineColumnRange.CombineAll(name.Select(id => id.SourceInfo).Concat(typeref.Select(tr => tr.SourceInfo)).Concat(initializer.FlatTokens.Select(ft => ft.SourceInfo)))));

        // { (local-vardecl|statement)* }
        public static readonly Parser<PartialBlock> BlockDecl =
            Parser.Combine(
                LiteralParser.OpenCurly,
                Parser.Delimited(LiteralParser.SemiColon, LocalVar.Select(lv => new Tuple<PartialStatement, VarDeclElement>(null, lv)).Or(Statement.Select(s => new Tuple<PartialStatement, VarDeclElement>(s, null)), "Statement or Local Variable"), false, true),
                LiteralParser.CloseCurly,
                (o, lines, c) => new PartialBlock(BuildStatements(lines).ToList(), lines.Where(t => t.Item2 != null).Select(t => t.Item2)));

        // function-phrase => type-expr block
        public static readonly Parser<PartialReductionDeclaration> FunctionDeclaration =
            Parser.Combine(
                FunctionPhrase,
                LiteralParser.FunctionArrow,
                TypeExpr,
                BlockDecl,
                (phrase, op, type, block) => new PartialReductionDeclaration(phrase, new PartialFunction(type, block, null)));

        // (id|thisParam)+ : type-ref := statement
        public static readonly Parser<VarDeclElement> FieldDeclaration =
            Parser.Combine(
                ID.Select(id => new PartialPhrasePart(id)).Or(thisParam, "Field name part").OneOrMore,
                LiteralParser.Colon,
                SimpleTypeReference,
                LiteralParser.InitializerEquals,
                Statement,
                LiteralParser.SemiColon,
                (name, c, typeref, ie, initializer, sc) => new VarDeclElement(new PartialParameterDeclaration(name, typeref.ToList()), initializer, LineColumnRange.CombineAll(name.Select(id => id.IsIdentifier ? id.Identifier.SourceInfo : null).Concat(typeref.Select(tr => tr.SourceInfo)).Concat(initializer.FlatTokens.Select(ft => ft.SourceInfo)))));
                

        public static readonly Parser<IEnumerable<Tuple<VarDeclElement, PartialReductionDeclaration>>> ClassBody = FieldDeclaration.Select(fd => new Tuple<VarDeclElement, PartialReductionDeclaration>(fd, null)).Or(FunctionDeclaration.Select(fd => new Tuple<VarDeclElement, PartialReductionDeclaration>(null, fd)), "Class member").ZeroOrMore;

        // (function-phrase - |) inline-interface-bindings? { class-body }
        public static readonly Parser<TangentType> ClassDecl =
            Parser.Combine(
                FunctionPhraseSansPipe,
                InlineInterfaceBinding.ZeroOrMore,
                LiteralParser.OpenCurly,
                ClassBody,
                LiteralParser.CloseCurly,
                (ctor, ifs, o, body, c) => ConstructProductType(ctor, ifs, body));

        // type-alias (| type-alias)* (;|class-decl)
        public static readonly Parser<TangentType> TypeAliasChain =
            Parser.Combine(
                Parser.Delimited(Pipe, Parser.NotFollowedBy(Parser.Difference(ID, Pipe).OneOrMore, LiteralParser.OpenCurly.Or(LiteralParser.InterfaceBindingOperator, "class start"), "Type Alias"), requiresOne: true, optionalTrailingDelimiter: false),
                LiteralParser.SemiColon.Select(sc => (TangentType)null).Or(Parser.Combine(Pipe, ClassDecl, (p, cd) => cd), "Semicolon or Class Declaration"),
                (aliases, optionalClass) => ConstructSumTypeFromAliasChain(aliases.Select(alias => new PartialTypeReference(alias, new List<PartialParameterDeclaration>())), optionalClass));

        // function-phrase => type-expr ;
        public static readonly Parser<PartialReductionDeclaration> InterfaceFunctionSignature =
            Parser.Combine(
                FunctionPhrase,
                LiteralParser.FunctionArrow,
                TypeExpr,
                LiteralParser.SemiColon,
                (phrase, op, type, sc) => new PartialReductionDeclaration(phrase, new PartialFunction(type, null, null)));

        // (id|type-param-decl)+ inline-interface-binding { function-decl* }
        public static readonly Parser<PartialInterfaceBinding> StandaloneInterfaceBinding =
            Parser.Combine(
                (ID.Select(expr => new PartialPhrasePart(expr)).Or(TypeDeclParam.Select(ppd => new PartialPhrasePart(ppd)), "Type Phrase Part").OneOrMore),
                InlineInterfaceBinding.OneOrMore,
                LiteralParser.OpenCurly,
                FunctionDeclaration.ZeroOrMore,
                LiteralParser.CloseCurly,
                (typeRef, ifs, o, fns, c) => ConstructStandaloneBinding(typeRef, ifs, fns));

        // interface { interface-function-signature+ }
        public static readonly Parser<TangentType> InterfaceDecl =
            Parser.Combine(
                Interface,
                LiteralParser.OpenCurly,
                InterfaceFunctionSignature.OneOrMore,
                LiteralParser.CloseCurly,
                (i, o, sigs, c) => (TangentType)ConstructInterface(sigs));

        public static readonly Parser<TangentType> TypeImpl =
            Parser.Options("Type Implementation",
                EnumImpl,
                InterfaceDecl,
                TypeAliasChain,
                ClassDecl);

        // (id|type-param)+ :> guts
        public static readonly Parser<PartialTypeDeclaration> TypeDecl = Parser.Combine(
            (ID.Select(expr => new PartialPhrasePart(expr)).Or(TypeDeclParam.Select(ppd => new PartialPhrasePart(ppd)), "Type Phrase Part").OneOrMore),
            LiteralParser.TypeArrow,
            TypeImpl,
            (phrase, arrow, impl) => ConstructTypeDeclaration(phrase, impl));

        private static TangentType ConstructSumTypeFromAliasChain(IEnumerable<TangentType> aliases, TangentType optionalClass)
        {
            if (optionalClass != null) {
                return SumType.For(aliases.Concat(new[] { optionalClass }));
            } else {
                if (aliases.Count() == 1) {
                    return aliases.First();
                }

                return SumType.For(aliases);
            }
        }

        private static TangentType ConstructProductType(IEnumerable<PartialPhrasePart> constructorBits, IEnumerable<TangentType> interfaceReferences, IEnumerable<Tuple<VarDeclElement,PartialReductionDeclaration>> body)
        {
            interfaceReferences = interfaceReferences ?? Enumerable.Empty<TangentType>();
            var fields = body.Where(e => e.Item1 != null).Select(e => e.Item1).ToList();
            var fns = body.Where(e => e.Item2 != null).Select(e => e.Item2).ToList();
            var result = new PartialProductType(constructorBits, fns, fields, new List<PartialParameterDeclaration>(), interfaceReferences);
            var boundFunctions = result.Functions.Select(fn => new PartialReductionDeclaration(fn.Takes, new PartialFunction(fn.Returns.EffectiveType, fn.Returns.Implementation, result))).ToList();
            result.Functions.Clear();
            result.Functions.AddRange(boundFunctions);
            return result;
        }

        private static TangentType ConstructInterface(IEnumerable<PartialReductionDeclaration> signatures)
        {
            var result = new PartialInterface(Enumerable.Empty<PartialReductionDeclaration>(), Enumerable.Empty<PartialParameterDeclaration>());
            var boundFunctions = signatures.Select(fn => new PartialReductionDeclaration(fn.Takes, new PartialFunction(fn.Returns.EffectiveType, fn.Returns.Implementation, result))).ToList();
            result.Functions.AddRange(boundFunctions);
            return result;
        }

        private static PartialInterfaceBinding ConstructStandaloneBinding(IEnumerable<PartialPhrasePart> typePhrase, IEnumerable<TangentType> interfaces, IEnumerable<PartialReductionDeclaration> functions)
        {
            var generics = typePhrase.Where(tp => !tp.IsIdentifier).Select(tp => tp.Parameter);
            var result = new PartialInterfaceBinding(typePhrase, interfaces.Cast<PartialTypeReference>().Select(ptr => new PartialTypeReference(ptr.Identifiers, generics)));
            // TODO: clean this stuff up.
            result.Functions.Clear();
            result.Functions.AddRange(functions.Select(fn => new PartialReductionDeclaration(fn.Takes, new PartialFunction(fn.Returns.EffectiveType, fn.Returns.Implementation, result))));
            return result;
        }

        private static PartialTypeDeclaration ConstructTypeDeclaration(IEnumerable<PartialPhrasePart> typePhrase, TangentType implementation)
        {
            // Take any generics and propogate to implementation bits.
            var generics = typePhrase.Where(ppp => !ppp.IsIdentifier).Select(ppp => ppp.Parameter).ToList();
            if (generics.Any()) {
                SetGenericParams(implementation, generics);
            }

            return new PartialTypeDeclaration(typePhrase, implementation);
        }

        private static IEnumerable<PartialStatement> BuildStatements(IEnumerable<Tuple<PartialStatement, VarDeclElement>> lines)
        {
            foreach (var entry in lines) {
                if (entry.Item1 != null) {
                    yield return entry.Item1;
                } else {

                    // otherwise, we need to initialize the local at this step.
                    yield return new PartialStatement(entry.Item2.ParameterDeclaration.Takes.Select(id => new IdentifierElement(id.Identifier.Identifier, id.Identifier.SourceInfo)).Concat(new[] { new IdentifierElement("=", null) }).Concat(entry.Item2.Initializer.FlatTokens));
                }
            }
        }

        private static void SetGenericParams(TangentType implementation, IEnumerable<PartialParameterDeclaration> generics)
        {
            var product = implementation as PartialProductType;
            if (product != null) {
                (product.GenericArguments as List<PartialParameterDeclaration>).AddRange(generics);
                return;
            }

            var reference = implementation as PartialTypeReference;
            if (reference != null) {
                (reference.GenericArgumentPlaceholders as List<PartialParameterDeclaration>).AddRange(generics);
                return;
            }

            var sum = implementation as SumType;
            if (sum != null) {
                foreach (var t in sum.Types) {
                    SetGenericParams(t, generics);
                }

                return;
            }

            return;
        }
    }
}
