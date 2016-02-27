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

        // (id+(:id+)?)
        public static readonly Parser<PartialParameterDeclaration> TypeDeclParam =
            Parser.Combine(
                LiteralParser.OpenParen,
                ID.OneOrMore,
                Parser.Combine(LiteralParser.Colon, ID.OneOrMore, (c, typeref) => typeref).Maybe(),
                LiteralParser.CloseParen,
                (o, phrase, typeref, c) => new PartialParameterDeclaration(phrase, typeref.Select(idexpr=>(Expression)idexpr).ToList() ?? new List<Expression>() { new IdentifierExpression("any", null) }));

        // enum { id+, id+, ... }
        public static readonly Parser<EnumType> EnumImpl =
            Parser.Combine(
                new StringLiteralParser("enum"),
                LiteralParser.OpenCurly,
                Parser.Delimited(new StringLiteralParser(","), ID.OneOrMore),
                LiteralParser.CloseCurly,
                (e, o, enums, c) => new EnumType(enums.Select(entry => entry.First().Identifier)));

        // type-alias (| type-alias)* (;|class-decl)
        public static readonly Parser<TangentType> TypeAliasChain =
            Parser.Combine(
                Parser.Delimited(Pipe, ID.OneOrMore),
                LiteralParser.SemiColon.Select(sc => (TangentType)null).Or(Parser.Combine(Pipe, ClassDecl, (p, cd) => cd)),
                (aliases, optionalClass) => ConstructTypeAliasChain(aliases, optionalClass));


        public static readonly Parser<TangentType> TypeImpl =
            Parser.Options("Type Implementation",
                EnumImpl,
//                InterfaceDecl,
                TypeAliasChain,
                ClassDecl);

        // (id|type-param)+ :> guts
        public static readonly Parser<PartialTypeDeclaration> TypeDecl = Parser.Combine(
            (ID.Select(expr => new PartialPhrasePart(expr)).Or(TypeDeclParam.Select(ppd=>new PartialPhrasePart(ppd)), "Type Phrase Part").OneOrMore()),
            LiteralParser.TypeArrow,
            TypeImpl,
            (phrase, arrow, impl) => new PartialTypeDeclaration(phrase, impl));

        private static TangentType ConstructTypeAliasChain(IEnumerable<TangentType> aliases, TangentType optionalClass)
        {
            throw new NotImplementedException();
        }
    }
}
