using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Patterns.Elements
{
    public class OptionalElement : PatternElement
    {
        public PatternElement Value { get; }

        public OptionalElement(PatternElement option)
        {
            Value = option;
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            if (Value is OptionalElement)
                throw new Exception("Nested optional elements don't add anything");

            Value.Validate(context);
        }

        protected override bool OnCanExecute(PatternState state)
        {
            return true;
        }

        protected override bool OnExecute(PatternState state)
        {
            if (Value.CanExecute(state))
                return Value.Execute(state);

            return true;
        }
    }
}
