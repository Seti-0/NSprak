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
    }
}
