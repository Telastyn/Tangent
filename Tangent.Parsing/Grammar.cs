using System;
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
        private static readonly Parser<IdentifierExpression> LazyOperator = LiteralParser.LazyOperator.Select(x => new IdentifierExpression("~>", null));
        public static readonly Parser<ConstantElement<string>> StringConstant = new StringConstantParser();
        public static readonly Parser<ConstantElement<int>> IntConstant = new IntConstantParser();

        // (id+(:id+)?)
        public static readonly Parser<PartialParameterDeclaration> TypeDeclParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                ID.OneOrMore,
                Parser.Combine(LiteralParser.Colon, ID.OneOrMore, (c, typeref) => typeref).Maybe,
                LiteralParser.CloseParen,
                (o, phrase, typeref, c) => new PartialParameterDeclaration(phrase, typeref.Select(idexpr => (Expression)idexpr).ToList() ?? new List<Expression>() { new IdentifierExpression("any", null) }));

        // enum { id+, id+, ... }
        public static readonly Parser<TangentType> EnumImpl =
            Parser.Combine(
                new StringLiteralParser("enum"),
                LiteralParser.OpenCurly,
                Parser.Delimited(new StringLiteralParser(","), ID.OneOrMore),
                LiteralParser.CloseCurly,
                (e, o, enums, c) => (TangentType)new EnumType(enums.Select(entry => entry.First().Identifier)));

        // (id|lazy)+
        public static readonly Parser<IEnumerable<Expression>> TypeExpr = ID.Or(LazyOperator, "Identifier").OneOrMore.Select(expr => expr.Select(id => (Expression)id));

        // (type-expr);
        public static readonly Parser<PartialPhrasePart> ParamParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                TypeExpr,
                LiteralParser.CloseParen,
                (o, expr, c) => new PartialPhrasePart(new PartialParameterDeclaration(new IdentifierExpression("", null), expr.ToList())));

        // (id|param-param)+
        public static readonly Parser<IEnumerable<PartialPhrasePart>> ParamNamePart = ID.Select(id => new PartialPhrasePart(id)).Or(ParamParam, "Parameter name part").OneOrMore;

        // (id+)
        public static readonly Parser<Expression> ParamInferencePlaceholder =
            Parser.Combine(
                LiteralParser.OpenParen,
                ID.OneOrMore,
                LiteralParser.CloseParen,
                (o, ids, c) => (Expression)new PartialTypeInferenceExpression(ids.Select(id => id.Identifier), new Expression[] { new IdentifierExpression("any", null) }, null));

        // (id|lazy|param-inference-placeholder)+
        public static readonly Parser<IEnumerable<Expression>> ParamTypePart =
            Parser.Options("Parameter type part",
                ID.Select(id => (Expression)id),
                LazyOperator.Select(id => (Expression)id),
                ParamInferencePlaceholder).OneOrMore;

        // ( name : type )
        public static readonly Parser<PartialPhrasePart> ParamDecl =
            Parser.Combine(
                LiteralParser.OpenParen,
                ParamNamePart,
                LiteralParser.Colon,
                ParamTypePart,
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

        // :< id+
        public static readonly Parser<TangentType> InlineInterfaceBinding =
            Parser.Combine(
                LiteralParser.InterfaceBindingOperator,
                ID.OneOrMore,
                (op, ids) => (TangentType)new PartialTypeReference(ids, Enumerable.Empty<PartialParameterDeclaration>()));

        public static readonly Parser<PartialElement> LambdaExpr =
            Parser.Combine(
                Parser.Options("Lambda parameters",
                    ID.Select(id => (IEnumerable<VarDeclElement>)new[] { new VarDeclElement(new PartialParameterDeclaration(id, null), id.SourceInfo) }),
                    Parser.Combine(LiteralParser.OpenParen, ID.OneOrMore, LiteralParser.CloseParen, (o, ids, c) => new VarDeclElement(new PartialParameterDeclaration(ids, null), LineColumnRange.CombineAll(ids.Select(id => id.SourceInfo)))).OneOrMore
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
                (o, exprs, c) => (PartialElement)new BlockElement(new PartialBlock(new[] { new PartialStatement(exprs) })));

        public static readonly Parser<PartialStatement> Statement =
            Parser.Combine(
                Expr.OneOrMore,
                LiteralParser.SemiColon,
                (exprs, sc) => new PartialStatement(exprs));

        // { statement* }
        public static readonly Parser<PartialBlock> BlockDecl =
            Parser.Combine(
                LiteralParser.OpenCurly,
                Statement.ZeroOrMore,
                LiteralParser.CloseCurly,
                (o, stmts, c) => new PartialBlock(stmts));

        // function-phrase => type-expr block
        public static readonly Parser<PartialReductionDeclaration> FunctionDeclaration =
            Parser.Combine(
                FunctionPhrase,
                LiteralParser.FunctionArrow,
                TypeExpr,
                BlockDecl,
                (phrase, op, type, block) => new PartialReductionDeclaration(phrase, new PartialFunction(type, block, null)));

        public static readonly Parser<IEnumerable<PartialReductionDeclaration>> ClassBody = FunctionDeclaration.ZeroOrMore;

        // (function-phrase - |) inline-interface-bindings? { class-body }
        public static readonly Parser<TangentType> ClassDecl =
            Parser.Combine(
                Parser.Difference(FunctionPhrase, Pipe),
                InlineInterfaceBinding.ZeroOrMore,
                LiteralParser.OpenCurly,
                ClassBody,
                LiteralParser.CloseCurly,
                (ctor, ifs, o, body, c) => ConstructProductType(ctor, ifs, body));

        // type-alias (| type-alias)* (;|class-decl)
        public static readonly Parser<TangentType> TypeAliasChain =
            Parser.Combine(
                Parser.Delimited(Pipe, ID.OneOrMore),
                LiteralParser.SemiColon.Select(sc => (TangentType)null).Or(Parser.Combine(Pipe, ClassDecl, (p, cd) => cd), "Semicolon or Class Declaration"),
                (aliases, optionalClass) => ConstructTypeAliasChain(aliases.Select(alias => new PartialTypeReference(alias, null)), optionalClass));


        public static readonly Parser<TangentType> TypeImpl =
            Parser.Options("Type Implementation",
                EnumImpl,
            //  InterfaceDecl,
                TypeAliasChain,
                ClassDecl);

        // (id|type-param)+ :> guts
        public static readonly Parser<PartialTypeDeclaration> TypeDecl = Parser.Combine(
            (ID.Select(expr => new PartialPhrasePart(expr)).Or(TypeDeclParam.Select(ppd => new PartialPhrasePart(ppd)), "Type Phrase Part").OneOrMore),
            LiteralParser.TypeArrow,
            TypeImpl,
            (phrase, arrow, impl) => ConstructTypeDeclaration(phrase, impl));

        private static TangentType ConstructTypeAliasChain(IEnumerable<TangentType> aliases, TangentType optionalClass)
        {
            throw new NotImplementedException();
        }

        private static TangentType ConstructProductType(IEnumerable<PartialPhrasePart> constructorBits, IEnumerable<TangentType> interfaceReferences, IEnumerable<PartialReductionDeclaration> body)
        {
            interfaceReferences = interfaceReferences ?? Enumerable.Empty<TangentType>();
            throw new NotImplementedException();
        }

        private static PartialTypeDeclaration ConstructTypeDeclaration(IEnumerable<PartialPhrasePart> typePhrase, TangentType implementation)
        {
            // Take any generics and propogate to implementation bits.
            throw new NotImplementedException();
        }
    }
}
