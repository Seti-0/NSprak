using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Language.Values;

namespace NSprak.Language
{
    public abstract class Value
    {
        public static bool IsBoolean(string text)
        {
            return text == "true" || text == "false";
        }

        public static bool IsNumber(string text)
        {
            return double.TryParse(text, out _);
        }

        public SprakString ToSprakString()
        {
            return new SprakString(ToFriendlyString());
        }

        public string FriendlyString => ToFriendlyString();

        public abstract SprakType Type { get; }

        public abstract string ToFriendlyString();

        public string ToDebugString()
        {
            return $"{ToFriendlyString()} ({Type.InternalName})";
        }

        public abstract Value Copy();

        public override string ToString()
        {
            return ToFriendlyString();
        }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public static bool operator ==(Value a, Value b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(Value a, Value b)
        {
            if (a is null) return !(b is null);
            return !a.Equals(b);
        }
    }
}
