using System;

namespace NSprak.Expressions.Patterns.Elements
{
    public class CommandElement : PatternElement
    {
        public PatternCommand Value { get; }

        public CommandElement(PatternCommand value)
        {
            Value = value;
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            // Nothing to check here.
        }

        protected override bool OnCanExecute(PatternState state)
        {
            return true;
        }

        protected override bool OnExecute(PatternState state)
        {
            state.SetCommand(Value);
            return true;
        }
    }
}
