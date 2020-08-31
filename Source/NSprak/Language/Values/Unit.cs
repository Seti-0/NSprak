using NSprak.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language.Values
{
    public class SprakUnit : Value
    {
        public static readonly SprakUnit Value = new SprakUnit();

        public override SprakType Type => SprakType.Unit;

        private SprakUnit() { }

        public override string ToFriendlyString()
        {
            return Type.Text;
        }

        public override Value Copy()
        {
            throw new SprakInternalRuntimeException($"Cannot copy the {nameof(SprakUnit)} value");
        }
    }
}
