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
    }
}
