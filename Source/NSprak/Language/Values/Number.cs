using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakNumber : Value
    {
        public double Value { get; }

        public override SprakType Type => SprakType.Number;

        public SprakNumber(double value = 0)
        {
            Value = value;
        }

        public override string ToFriendlyString()
        {
            return Value.ToString();
        }

        public override Value Copy()
        {
            return new SprakNumber(Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is SprakNumber other)
                return other.Value == Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(SprakNumber a, SprakNumber b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(SprakNumber a, SprakNumber b)
        {
            if (a is null) return !(b is null);
            return !a.Equals(b);
        }
    }
}
