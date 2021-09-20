using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakBoolean : Value
    {
        public bool Value { get; }

        public override SprakType Type => SprakType.Boolean;

        public SprakBoolean(bool value = false)
        {
            Value = value;
        }

        public override string ToFriendlyString()
        {
            return Value ? "true" : "false";
        }

        public override Value Copy()
        {
            return new SprakBoolean(Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SprakBoolean other)
                return other.Value == Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(SprakBoolean a, SprakBoolean b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(SprakBoolean a, SprakBoolean b)
        {
            if (a is null) return !(b is null);
            return !a.Equals(b);
        }
    }
}
