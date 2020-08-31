using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakArray : Value
    {
        public List<Value> Value { get; private set; }

        public override SprakType Type => SprakType.Array;

        public SprakArray(List<Value> value = null)
        {
            Value = value ?? new List<Value>();
        }

        public override string ToFriendlyString()
        {
            var toStrings = Value.Select(x => x.ToFriendlyString());
            string body = string.Join(",", toStrings);

            return $"[{body}]";
        }

        public override Value Copy()
        {
            List<Value> newValues = Value
                .Select(x => x.Copy())
                .ToList();

            return new SprakArray(newValues);
        }
    }
}
