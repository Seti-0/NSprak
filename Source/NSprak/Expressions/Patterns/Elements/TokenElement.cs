using System;

using NSprak.Tokens;

namespace NSprak.Expressions.Patterns.Elements
{
    public class TokenElement : PatternElement
    {
        public TokenType Type { get; }
        public string Content { get; }

        public TokenElement(TokenType type, string content)
        {
            Type = type;
            Content = content;
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            // Nothing to check here.
        }

        protected override bool OnCanExecute(PatternState state)
        {
            return state.Enumerator.HasCurrent
                && state.Enumerator.Current.Type == Type
                && state.Enumerator.Current.Content == Content;
        }

        protected override bool OnExecute(PatternState state)
        {
            state.AddToCollection(state.Enumerator.Current);
            state.Enumerator.MoveNext();
            return true;
        }
    }
}
