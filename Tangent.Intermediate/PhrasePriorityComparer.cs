using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ExpressionDeclarationPriorityComparer : IComparer<ExpressionDeclaration>
    {
        private static PhrasePriorityComparer phraseComparer = new PhrasePriorityComparer();

        public int Compare(ExpressionDeclaration x, ExpressionDeclaration y)
        {
            return phraseComparer.Compare(x.DeclaredPhrase, y.DeclaredPhrase);
        }
    }

    public class PhrasePriorityComparer : IComparer<Phrase>
    {
        public static readonly PhrasePriorityComparer Common = new PhrasePriorityComparer();

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
                var ppp = ComparePhrasePartPriority(xenum.Current, yenum.Current);
                if (ppp != null) {
                    return ppp.Value;
                }
            }

            return 0;
        }

        public static int? ComparePhrasePartPriority(PhrasePart x, PhrasePart y)
        {
            if (x.IsIdentifier) {
                if (y.IsIdentifier) {
                    if (x.Identifier.Value != y.Identifier.Value) {
                        return 0;
                    } else {
                        return null;
                    }
                } else {
                    return -1;
                }
            } else {
                if (y.IsIdentifier) {
                    return 1;
                } else {
                    var xp = x.Parameter;
                    var yp = y.Parameter;
                    var enumResult = CompareEnum(xp, yp);
                    if (enumResult != null) { return enumResult.Value; }

                    var genericResult = CompareGeneric(xp, yp);
                    if (genericResult != null) { return genericResult.Value; }

                    return null;
                }
            }
        }

        private static int? CompareEnum(ParameterDeclaration x, ParameterDeclaration y)
        {
            if (x.RequiredArgumentType.ImplementationType == KindOfType.Enum && y.RequiredArgumentType.ImplementationType == KindOfType.SingleValue) {
                var xenum = x.RequiredArgumentType as EnumType;
                var ysvt = y.RequiredArgumentType as SingleValueType;
                if (xenum == ysvt.ValueType) {
                    return 1;
                } else {
                    return 0;
                }
            } else if (x.RequiredArgumentType.ImplementationType == KindOfType.SingleValue && y.RequiredArgumentType.ImplementationType == KindOfType.Enum) {
                var xsvt = x.RequiredArgumentType as SingleValueType;
                var yenum = y.RequiredArgumentType as EnumType;
                if (yenum == xsvt.ValueType) {
                    return -1;
                } else {
                    return 0;
                }
            }

            return null;
        }

        public static int? CompareGeneric(ParameterDeclaration x, ParameterDeclaration y)
        {
            return CompareGeneric(x.RequiredArgumentType, y.RequiredArgumentType);
        }

        public static int? CompareGeneric(TangentType x, TangentType y)
        {
            // Taking this from exising implementation. May be better ways to do it.

            // Non-generics preferred.
            // Generics that can infer the other are considered more general, and thus less preferred.

            var isXGeneric = x.ContainedGenericReferences().Any();
            var isYGeneric = y.ContainedGenericReferences().Any();

            if (isXGeneric) {
                if (!isYGeneric) {
                    return 1;
                }

                var xCanInferY = x.CompatibilityMatches(y, new Dictionary<ParameterDeclaration, TangentType>());
                var yCanInferX = y.CompatibilityMatches(x, new Dictionary<ParameterDeclaration, TangentType>());
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
