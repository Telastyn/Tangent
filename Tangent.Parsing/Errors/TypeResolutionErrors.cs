using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Errors {
    public class TypeResolutionErrors : ParseError {
        public readonly IEnumerable<BadTypePhrase> Errors;
        public TypeResolutionErrors(IEnumerable<BadTypePhrase> errors) {
            Errors = errors;
        }
    }
}
