namespace NSprak.Expressions.Patterns.Elements
{
    public class EmptyElement : PatternElement
    {
        protected override void OnValidate(PatternCheckContext context)
        {
            // Nothing to check here.
        }

        protected override bool OnCanExecute(PatternState state)
        {
            return !state.Enumerator.HasCurrent;
        }

        protected override bool OnExecute(PatternState state)
        {
            // Nothing that needs to be done here.
            return true;
        }
    }
}
