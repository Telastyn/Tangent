using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent
{
    public class LineColumnPosition : IComparable<LineColumnPosition>
    {
        public readonly int Line;
        public readonly int Column;

        internal LineColumnPosition(int line, int column)
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

        public int CompareTo(LineColumnPosition other)
        {
            int result = Line.CompareTo(other.Line);
            if (result == 0) {
                result = Column.CompareTo(other.Column);
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("(line: {0}, column: {1})", Line, Column);
        }
    }
}
