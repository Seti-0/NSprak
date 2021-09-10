using System;

namespace NSprak.Expressions.Patterns.Elements
{
    public class EndElement : PatternElement
    {
        public PatternEnd Destination { get; }

        public EndElement(PatternEnd destination)
        {
            Destination = destination;
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
            state.End(Destination);
            return true;
        }
    }
}
