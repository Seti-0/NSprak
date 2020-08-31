using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakBoolean : Value
    {
        public bool Value { get; private set; }

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
    }
}
