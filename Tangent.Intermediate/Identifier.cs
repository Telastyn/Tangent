using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Intermediate {
    public class Identifier {
        public readonly string Value;

        public Identifier(string value) {
            Value = value;
        }

        public static bool operator ==(Identifier me, Identifier other) {
            if (object.ReferenceEquals(me, null)) {
                return object.ReferenceEquals(other, null);
            }

            return me.Equals(other);
        }

        public static bool operator !=(Identifier me, Identifier other) {
            return !(me == other);
        }

        public override bool Equals(object obj) {
            if (object.Equals(obj, null)) { return false; }
            Identifier objIdentifier = obj as Identifier;
            if (!object.ReferenceEquals(objIdentifier, null)) {
                return this.Value == objIdentifier.Value;
            }

            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public static implicit operator Identifier(string value) {
            return new Identifier(value);
        }
    }
}
