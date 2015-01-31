using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Tokenization
{
    public class LineColumnPosition
    {
        public readonly int Line;
        public readonly int Column;

        private LineColumnPosition(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }

        public static LineColumnPosition Create(string input, int index)
        {
            if (index < 0 || index > input.Length) {
                throw new ArgumentOutOfRangeException("index");
            }

            var substring = input.Substring(0, index);
            int lines = 1;
            int last = -1;
            for (int ix = 0; ix < index; ++ix) {
                if (input[ix] == '\n') {
                    lines++;
                    last = ix;
                }
            }

            return new LineColumnPosition(lines, index - last);
        }
    }
}
