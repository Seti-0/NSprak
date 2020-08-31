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
    }
}
