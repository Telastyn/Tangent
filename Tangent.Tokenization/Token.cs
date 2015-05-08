using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Tokenization
{
    public enum TokenIdentifier
    {
        Identifier,
        Symbol,

        TypeDeclSeparator,
        ReductionDeclSeparator,
        
        LazyOperator,
        StringConstant,
        IntegerConstant,
    }

    [Serializable]
    public class Token
    {
        private readonly string input;
        internal int StartIndex { get; private set; }
        internal int EndIndex { get; private set; }
        public readonly LineColumnRange SourceInfo;

        public Token(TokenIdentifier id, string input, int startIndex, int endIndex, string inputLabel)
        {
            this.Identifier = id;
            this.input = input;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.SourceInfo = new LineColumnRange(inputLabel, input, startIndex, endIndex);
        }

        public TokenIdentifier Identifier
        {
            get;
            private set;
        }

        public string Value
        {
            get { return input.Substring(StartIndex, EndIndex - StartIndex); }
        }
    }
}
