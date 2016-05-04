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

        public bool MatchesSignatureOf(ReductionDeclaration rhs)
        {
            // TODO: generic returns.
            if (this.Returns.EffectiveType != rhs.Returns.EffectiveType) { return false; }
            if (this.Takes.Count != rhs.Takes.Count) { return false; }
            if (GenericParameters.Count() != rhs.GenericParameters.Count()) { return false; }
            if (!GenericParameters.SequenceEqual(rhs.GenericParameters, (a, b) => a.Returns == b.Returns)) { return false; }

            var genericMap = GenericParameters.Zip(rhs.GenericParameters, (a, b) => Tuple.Create(a, b)).ToDictionary(x => x.Item1, x => x.Item2);

            foreach (var entry in this.Takes.Zip(rhs.Takes, (a, b) => Tuple.Create(a, b))) {
                if (entry.Item1.IsIdentifier != entry.Item2.IsIdentifier) { return false; }
                if (entry.Item1.IsIdentifier) {
                    if (entry.Item1.Identifier.Value != entry.Item2.Identifier.Value) {
                        return false;
                    }
                } else {
                    // TODO: generic mappings.
                    if (entry.Item1.Parameter.Returns != entry.Item2.Parameter.Returns) { return false; }
                }
            }

            return true;
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
                    var rhsInferences = rhsEnum.Current.IsIdentifier ? Enumerable.Empty<ParameterDeclaration>() : rhsEnum.Current.Parameter.RequiredArgumentType.ContainedGenericReferences(GenericTie.Inference);
                    if (!rhsEnum.Current.IsIdentifier && rhsInferences.Any()) {
                        var necessaryInferences = new Dictionary<ParameterDeclaration, TangentType>();
                        if (rhsEnum.Current.Parameter.RequiredArgumentType.CompatibilityMatches(thisEnum.Current.Parameter.RequiredArgumentType, necessaryInferences)) {
                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter, necessaryInferences);
                        } else {
                            yield return null;
                            yield break;
                        }
                    } else if (!rhsEnum.Current.IsIdentifier && rhsEnum.Current.Parameter.RequiredArgumentType.ImplementationType == KindOfType.Sum) {
                        if (rhsEnum.Current.Parameter.RequiredArgumentType == thisEnum.Current.Parameter.RequiredArgumentType) {
                            break;
                        }

                        if (!((SumType)rhsEnum.Current.Parameter.RequiredArgumentType).Types.Contains(thisEnum.Current.Parameter.RequiredArgumentType)) {
                            yield return null;
                            yield break;
                        } else {
                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                        }

                        break;
                    } else if (!rhsEnum.Current.IsIdentifier && rhsEnum.Current.Parameter.RequiredArgumentType.ImplementationType == KindOfType.TypeClass) {
                        if (rhsEnum.Current.Parameter.RequiredArgumentType == thisEnum.Current.Parameter.RequiredArgumentType) {
                            break;
                        }

                        if (((TypeClass)rhsEnum.Current.Parameter.RequiredArgumentType).Implementations.Contains(thisEnum.Current.Parameter.RequiredArgumentType)) {
                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                        } else {
                            yield return null;
                            yield break;
                        }

                    } else {

                        switch (thisEnum.Current.Parameter.RequiredArgumentType.ImplementationType) {
                            case KindOfType.SingleValue:
                                var single = (SingleValueType)thisEnum.Current.Parameter.RequiredArgumentType;
                                switch (rhsEnum.Current.Parameter.RequiredArgumentType.ImplementationType) {
                                    case KindOfType.SingleValue:
                                        var rhsSingle = (SingleValueType)rhsEnum.Current.Parameter.RequiredArgumentType;
                                        if (rhsSingle.ValueType != single.ValueType || rhsSingle.Value != single.Value) {
                                            yield return null;
                                            yield break;
                                        } else {
                                            yield return new SpecializationEntry(rhsEnum.Current.Parameter, thisEnum.Current.Parameter);
                                        }

                                        break;

                                    case KindOfType.Enum:
                                        if (single.ValueType != rhsEnum.Current.Parameter.RequiredArgumentType) {
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
                                if (thisEnum.Current.Parameter.RequiredArgumentType != rhsEnum.Current.Parameter.RequiredArgumentType) {
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

