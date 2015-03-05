using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent
{
    public class LineColumnRange
    {
        public readonly LineColumnPosition StartPosition;
        public readonly LineColumnPosition EndPosition;
        public readonly string Label;

        public LineColumnRange(string label, string input, int startIndex, int endIndex)
        {
            if (startIndex > endIndex) { throw new InvalidOperationException(); }

            Label = label;
            StartPosition = LineColumnPosition.Create(input, startIndex);
            EndPosition = LineColumnPosition.Create(input, endIndex);
        }

        private LineColumnRange(string label, LineColumnPosition start, LineColumnPosition end)
        {
            Label = label;
            StartPosition = start;
            EndPosition = end;
        }

        public LineColumnRange Combine(LineColumnRange other)
        {
            if (other == null) { return new LineColumnRange(Label, StartPosition, EndPosition); }
            if (this.Label != other.Label) {
                throw new InvalidOperationException("LineColumnRange.Label must match in order to merge.");
            }

            var startCmp = StartPosition.CompareTo(other.StartPosition);
            var endCmp = EndPosition.CompareTo(other.EndPosition);
            return new LineColumnRange(Label, startCmp <= 0 ? StartPosition : other.StartPosition, endCmp > 0 ? EndPosition : other.EndPosition);
        }

        public static LineColumnRange CombineAll(IEnumerable<LineColumnRange> ranges)
        {
            if (ranges == null) { return null; }
            ranges = ranges.Where(r => r != null);
            if (!ranges.Any()) { return null; }

            return ranges.Aggregate((LineColumnRange)null, (a, r) => r.Combine(a));
        }

        public LineColumnRange Combine(IEnumerable<LineColumnRange> ranges)
        {
            return this.Combine(ranges.Combine());
        }

        public static LineColumnRange Combine(LineColumnRange initial, IEnumerable<LineColumnRange> ranges)
        {
            if (initial == null) { return ranges.Combine(); }
            return initial.Combine(ranges.Combine());
        }

        public override string ToString()
        {
            return string.Format("{0} {1}-{2}", Label, StartPosition, EndPosition);
        }
    }

    public static class ExtendLineColumnRangeCollection
    {
        public static LineColumnRange Combine(this IEnumerable<LineColumnRange> ranges)
        {
            return LineColumnRange.CombineAll(ranges);
        }
    }
}
