using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakString : Value
    {
        public string Value { get; } = "";

        public override SprakType Type => SprakType.String;

        public SprakString(string value = "")
        {
            Value = value ?? "";
        }

        public override string ToFriendlyString()
        {
            return Value;
        }

        public override Value Copy()
        {
            return new SprakString(Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SprakString other)
                return other.Value == Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(SprakString a, SprakString b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(SprakString a, SprakString b)
        {
            if (a is null) return !(b is null);
            return !a.Equals(b);
        }
    }
}
