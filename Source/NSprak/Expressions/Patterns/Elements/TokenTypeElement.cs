using System;

using NSprak.Tokens;

namespace NSprak.Expressions.Patterns.Elements
{
    public class TokenTypeElement : PatternElement
    {
        public TokenType Type { get; }

        public TokenTypeElement(TokenType type)
        {
            Type = type;
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            // Nothing to check here.
        }

        protected override bool OnCanExecute(PatternState state)
        {
            return state.Enumerator.HasCurrent 
                && state.Enumerator.Current.Type == Type;
        }

        protected override bool OnExecute(PatternState state)
        {
            state.AddToCollection(state.Enumerator.Current);
            state.Enumerator.MoveNext();
            return true;
        }
    }
}
