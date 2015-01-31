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
        public ReductionDeclaration(IEnumerable<PhrasePart> takes, Function returns) : base(takes, returns) { }

        public bool IsSpecializationOf(ReductionDeclaration rhs)
        {
            if (this == rhs) { return false; }
            if (this.Returns.EffectiveType != rhs.Returns.EffectiveType) { return false; }
            if (this.Takes.Count != rhs.Takes.Count) { return false; }

            var thisEnum = this.Takes.GetEnumerator();
            var rhsEnum = rhs.Takes.GetEnumerator();
            while (thisEnum.MoveNext() && rhsEnum.MoveNext())
            {
                if (thisEnum.Current.IsIdentifier)
                {
                    if (rhsEnum.Current.IsIdentifier)
                    {
                        if (thisEnum.Current.Identifier.Value != rhsEnum.Current.Identifier.Value)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    switch (thisEnum.Current.Parameter.Returns.ImplementationType)
                    {
                        case KindOfType.SingleValue:
                            var single = (SingleValueType)thisEnum.Current.Parameter.Returns;
                            switch (rhsEnum.Current.Parameter.Returns.ImplementationType)
                            {
                                case KindOfType.SingleValue:
                                    var rhsSingle = (SingleValueType)rhsEnum.Current.Parameter.Returns;
                                    if (rhsSingle.ValueType != single.ValueType || rhsSingle.Value != single.Value)
                                    {
                                        return false;
                                    }

                                    break;

                                case KindOfType.Enum:
                                    if (single.ValueType != rhsEnum.Current.Parameter.Returns)
                                    {
                                        return false;
                                    }

                                    break;

                                default:
                                    return false;
                            }

                            break;

                        default:
                            if (thisEnum.Current.Parameter.Returns != rhsEnum.Current.Parameter.Returns)
                            {
                                return false;
                            }

                            break;
                    }
                }
            }

            return true;
        }
    }
}
