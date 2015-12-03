using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class PhrasePriorityComparer : IComparer<Phrase>
    {
        /// <summary>
        /// Compares the two phrases in priority order. "Less" phrases are higher priority. 0 compare is not necessarily equal, but disjoint.
        /// </summary>
        public int Compare(Phrase x, Phrase y) { return ComparePriority(x, y); }

        public static int ComparePriority(Phrase x, Phrase y)
        {
            var ctCmp = x.Pattern.Count().CompareTo(y.Pattern.Count());
            if (ctCmp < 0) { return 1; }
            if (ctCmp > 0) { return -1; }

            var xenum = x.Pattern.GetEnumerator();
            var yenum = y.Pattern.GetEnumerator();

            while (xenum.MoveNext() && yenum.MoveNext()) {
                if (xenum.Current.IsIdentifier) {
                    if (yenum.Current.IsIdentifier) {
                        if (xenum.Current.Identifier.Value != yenum.Current.Identifier.Value) {
                            return 0;
                        } // else, continue.
                    } else {
                        return -1;
                    }
                } else {
                    if (yenum.Current.IsIdentifier) {
                        return 1;
                    } else {
                        var xp = xenum.Current.Parameter;
                        var yp = yenum.Current.Parameter;
                        var enumResult = CompareEnum(xp,yp);
                        if (enumResult != null) { return enumResult.Value; }

                        var genericResult = CompareGeneric(xp, yp);
                        if (genericResult != null) { return genericResult.Value; }
                    }
                }
            }

            return 0;
        }

        private static int? CompareEnum(ParameterDeclaration x, ParameterDeclaration y)
        {
            if (x.Returns.ImplementationType == KindOfType.Enum && y.Returns.ImplementationType == KindOfType.SingleValue) {
                var xenum = x.Returns as EnumType;
                var ysvt = y.Returns as SingleValueType;
                if (xenum == ysvt.ValueType) {
                    return 1;
                } else {
                    return 0;
                }
            } else if (x.Returns.ImplementationType == KindOfType.SingleValue && y.Returns.ImplementationType == KindOfType.Enum) {
                var xsvt = x.Returns as SingleValueType;
                var yenum = y.Returns as EnumType;
                if (yenum == xsvt.ValueType) {
                    return -1;
                } else {
                    return 0;
                }
            }

            return null;
        }

        private static int? CompareGeneric(ParameterDeclaration x, ParameterDeclaration y)
        {
            // Taking this from exising implementation. May be better ways to do it.

            // Non-generics preferred.
            // Generics that can infer the other are considered more general, and thus less preferred.

            var isXGeneric = x.Returns.ContainedGenericReferences(GenericTie.Inference).Any();
            var isYGeneric = y.Returns.ContainedGenericReferences(GenericTie.Inference).Any();

            if (isXGeneric) {
                if (!isYGeneric) {
                    return 1;
                }

                var xCanInferY = x.Returns.CompatibilityMatches(y.Returns, new Dictionary<ParameterDeclaration, TangentType>());
                var yCanInferX = y.Returns.CompatibilityMatches(x.Returns, new Dictionary<ParameterDeclaration, TangentType>());
                if (xCanInferY) {
                    if (yCanInferX) {
                        return 0;
                    } else {
                        return 1;
                    }
                } else {
                    if (yCanInferX) {
                        return -1;
                    } else {
                        return 0;
                    }
                }
            } else if (isYGeneric) {
                return -1;
            }

            return null;
        }
    }
}
