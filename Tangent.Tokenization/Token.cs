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
        
        LazyOperator
    }

    [Serializable]
    public class Token
    {
        private readonly string input;
        internal int StartIndex { get; private set; }
        internal int EndIndex { get; private set; }

        public Token(TokenIdentifier id, string input, int startIndex, int endIndex)
        {
            this.Identifier = id;
            this.input = input;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
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

        public LineColumnPosition StartPosition
        {
            get
            {
                return LineColumnPosition.Create(input, StartIndex);
            }
        }

        public LineColumnPosition EndPosition
        {
            get
            {
                return LineColumnPosition.Create(input, EndIndex);
            }
        }
    }
}
