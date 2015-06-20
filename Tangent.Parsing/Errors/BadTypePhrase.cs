using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;

namespace Tangent.Parsing.Errors
{
    public enum BadTypePhraseReason
    {
        Unknown,
        Ambiguous,
        Incomprehensible
    }

    public class BadTypePhrase
    {
        public readonly IEnumerable<Identifier> TypePhrase;
        public readonly BadTypePhraseReason Reason;

        public BadTypePhrase(IEnumerable<Identifier> identifiers, BadTypePhraseReason reason)
        {
            TypePhrase = identifiers;
            Reason = reason;
        }
    }
}
