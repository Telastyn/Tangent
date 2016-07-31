using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ResultPriorityComparer : IComparer<TransformationResult>
    {
        public int Compare(TransformationResult x, TransformationResult y)
        {
            return ResultPriorityComparer.ComparePriority(x, y);
        }

        public static int ComparePriority(TransformationResult x, TransformationResult y)
        {
            // TODO: add unit tests for this, you lazy bum.
            // Ignoring success for now as a microoptimization.
            var takeCmp = x.Takes.CompareTo(y.Takes);
            if (takeCmp != 0) { return takeCmp; } // TODO: should always be 0? Eliminate.

            var xEnum = x.ConversionInfo.GetEnumerator();
            var yEnum = y.ConversionInfo.GetEnumerator();
            while (xEnum.MoveNext()) {
                if (!yEnum.MoveNext()) {
                    return 1;
                }

                if (xEnum.Current == null) {
                    if (yEnum.Current != null) {
                        return -1;
                    }
                } else {
                    if (yEnum.Current == null) {
                        return 1;
                    }

                    if (xEnum.Current.IsGeneric) {
                        if (!yEnum.Current.IsGeneric) {
                            return 1;
                        }
                    } else {
                        if (yEnum.Current.IsGeneric) {
                            return -1;
                        }
                    }

                    var costCmp = xEnum.Current.Cost.CompareTo(yEnum.Current.Cost);
                    if (costCmp != 0) { return costCmp; }
                }
            }

            if (yEnum.MoveNext()) {
                return -1;
            }

            return 0;
        }
    }
}
