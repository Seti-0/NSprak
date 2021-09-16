using System;

using NSprak.Tokens;
using NSprak.Language;

using NSprak.Expressions.Patterns.Elements;

namespace NSprak.Expressions.Patterns
{
    public static class PatternSyntax
    {
        public static PatternElement

            Boolean = new TokenTypeElement(TokenType.Boolean),
            Number = new TokenTypeElement(TokenType.Number),
            Text = new TokenTypeElement(TokenType.String),

            OperatorToken = new TokenTypeElement(TokenType.Operator),
            Name = new TokenTypeElement(TokenType.Name),
            Type = new TokenTypeElement(TokenType.Type),

            Break = new CommandElement(PatternCommand.Break),
            Loopback = new CommandElement(PatternCommand.Loopback),

            Empty = new EmptyElement();

        public static class Keyword
        {
            public static PatternElement
                Break = With(Keywords.Break),
                Continue = With(Keywords.Continue),
                Else = With(Keywords.Else),
                End = With(Keywords.End),
                From = With(Keywords.From),
                If = With(Keywords.If),
                In = With(Keywords.In),
                Loop = With(Keywords.Loop),
                Return = With(Keywords.Return),
                To = With(Keywords.To);

            private static PatternElement With(string word)
            {
                return new TokenElement(TokenType.KeyWord, word);
            }
        }

        public static class KeySymbol
        {
            public static PatternElement
                CloseBracket = From(Symbols.CloseBracket),
                CloseSquareBracket = From(Symbols.CloseSquareBracket),
                Comma = From(Symbols.Comma),
                CommentStart = From(Symbols.CommentStart),
                DecimalPoint = From(Symbols.DecimalPoint),
                Minus = From(Symbols.Minus),
                OpenBracket = From(Symbols.OpenBracket),
                OpenSquareBracket = From(Symbols.OpenSquareBracket),
                StringBoundary = From(Symbols.StringBoundary),
                StringBoundaryAlternate = From(Symbols.StringBoundaryAlternate);

            private static PatternElement From(char character)
            {
                return new TokenElement(TokenType.KeySymbol, character.ToString());
            }
        }

        public static Pattern Pattern(string name)
        {
            return new Pattern(name);
        }

        public static PatternElement Allow(PatternElement element)
        {
            return new OptionalElement(element);
        }

        public static PatternElement EndWith(PatternEnd iterator)
        {
            return new EndElement(iterator);
        }
    }
}
