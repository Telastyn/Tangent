using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate
{
    public class ReductionDeclaration : ReductionRule<PhrasePart, Function>
    {
        public ReductionDeclaration(Identifier takes, Function returns) : this(new[] { new PhrasePart(takes) }, returns) { }
        public ReductionDeclaration(PhrasePart takes, Function returns) : this(new[] { takes }, returns) { }
        public ReductionDeclaration(IEnumerable<PhrasePart> takes, Function returns) : this(takes, returns, Enumerable.Empty<ParameterDeclaration>()) { }
        public ReductionDeclaration(IEnumerable<PhrasePart> takes, Function returns, IEnumerable<ParameterDeclaration> genericParameters)
            : base(takes, returns)
        {
            if (!genericParameters.All(pd => pd.Returns.ImplementationType == KindOfType.Kind)) { throw new InvalidOperationException("Generic arguments to functions must have Kind types."); }
            GenericParameters = genericParameters;
        }

        public readonly IEnumerable<ParameterDeclaration> GenericParameters;

        public bool IsConversion
        {
            get
            {
                if (Takes.Count != 1) {
                    return false;
                }

                return !Takes.First().IsIdentifier;
            }
        }

        public override string SeparatorToken
        {
            get { return "=>"; }
        }

        public bool IsSpecializationOf(ReductionDeclaration rhs)
        {
            return SpecializationAgainst(rhs) != null;
        }

        public SpecializationDefinition SpecializationAgainst(ReductionDeclaration rhs)
        {
            var result = SpecializationsFor(rhs).ToList();
            if (result.Any(r => r == null)) { return null; }
            if (!result.Any()) { throw new ApplicationException("Some error has happened in specialization logic. Fix and test."); }
            return new SpecializationDefinition(result);
        }

        private IEnumerable<SpecializationEntry> SpecializationsFor(ReductionDeclaration rhs)
        {
            if (this == rhs) { yield return null; yield break; }
            if (this.Returns.EffectiveType != rhs.Returns.EffectiveType) { yield return null; yield break; }
            if (this.Takes.Count != rhs.Takes.Count) { yield return null; yield break; }

            var thisEnum = this.Takes.GetEnumerator();
            var rhsEnum = rhs.Takes.GetEnumerator();
            while (thisEnum.MoveNext() && rhsEnum.MoveNext()) {
                if (thisEnum.Current.IsIdentifier) {
                    if (rhsEnum.Current.IsIdentifier) {
                        if (thisEnum.Current.Identifier.Value != rhsEnum.Current.Identifier.Value) {
                            yield return null;
                            yield break;
                        }
                    } else {
                        yield return null;
                        yield break;
                    }
                } else {
                    var rhsInferences = rhsEnum.Current.IsIdentifier ? Enumerable.Empty<ParameterDeclaration>() : rhsEnum.Current.Parameter.Returns.ContainedGenericReferences(GenericTie.Inference);
                    if (!rhsEnum.Current.IsIdentifier && rhsInferences.Any()) {
                        var necessaryInferences = new Dictionary<ParameterDeclaration, TangentType>();
                        if (rhsEnum.Current.Parameter.Returns.CompatibilityMatches(thisEnum.Current.Parameter.Returns, necessaryInferences)) {
                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter, necessaryInferences);
                        } else {
                            yield return null;
                            yield break;
                        }
                    } else if (!rhsEnum.Current.IsIdentifier && rhsEnum.Current.Parameter.Returns.ImplementationType == KindOfType.Sum) {
                        if (rhsEnum.Current.Parameter.Returns == thisEnum.Current.Parameter.Returns) {
                            break;
                        }

                        if (!((SumType)rhsEnum.Current.Parameter.Returns).Types.Contains(thisEnum.Current.Parameter.Returns)) {
                            yield return null;
                            yield break;
                        } else {
                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                        }

                        break;
                    } else {
                        switch (thisEnum.Current.Parameter.Returns.ImplementationType) {
                            case KindOfType.SingleValue:
                                var single = (SingleValueType)thisEnum.Current.Parameter.Returns;
                                switch (rhsEnum.Current.Parameter.Returns.ImplementationType) {
                                    case KindOfType.SingleValue:
                                        var rhsSingle = (SingleValueType)rhsEnum.Current.Parameter.Returns;
                                        if (rhsSingle.ValueType != single.ValueType || rhsSingle.Value != single.Value) {
                                            yield return null;
                                            yield break;
                                        } else {
                                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                                        }

                                        break;

                                    case KindOfType.Enum:
                                        if (single.ValueType != rhsEnum.Current.Parameter.Returns) {
                                            yield return null;
                                            yield break;
                                        } else {
                                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                                        }

                                        break;

                                    default:
                                        yield return null;
                                        yield break;
                                }

                                break;

                            default:
                                if (thisEnum.Current.Parameter.Returns != rhsEnum.Current.Parameter.Returns) {
                                    yield return null;
                                    yield break;
                                }

                                break;
                        }
                    }
                }
            }
        }
    }
}
