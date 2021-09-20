using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakConnection : Value
    {
        public string ConnectionString { get; } = null;

        public override SprakType Type => SprakType.Connection;

        public SprakConnection(string connectionString = null)
        {
            ConnectionString = connectionString;
        }

        public override string ToFriendlyString()
        {
            return ConnectionString ?? "null";
        }

        public override Value Copy()
        {
            return new SprakConnection(ConnectionString);
        }

        public override bool Equals(object obj)
        {
            if (obj is SprakConnection other)
                return other.ConnectionString == ConnectionString;

            return false;
        }

        public override int GetHashCode()
        {
            return ConnectionString.GetHashCode();
        }

        public static bool operator ==(SprakConnection a, SprakConnection b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(SprakConnection a, SprakConnection b)
        {
            if (a is null) return !(b is null);
            return !a.Equals(b);
        }
    }
}
