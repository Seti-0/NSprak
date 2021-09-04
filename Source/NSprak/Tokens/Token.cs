using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NSprak.Exceptions;
using NSprak.Language;
using Microsoft.VisualBasic.CompilerServices;
using NSprak.Language.Values;

namespace NSprak.Tokens
{
    public enum TokenType
    {
        Comment,

        KeyWord,
        KeySymbol,

        Type,
        Name,

        Operator,

        String,
        Number,
        Boolean,

        Unknown
    }

    [Flags]
    public enum TokenHints
    {
        None = 0,
        Function = 1,
        UserFunction = 2,
        BuiltInFunction = 4,
    }

    public class Token
    {
        public TokenPage Page { get; }

        public PageLine Line { get; }

        public int LineNumber => Line.LineNumber;

        public int ColumnStart { get; }

        public int ColumnEnd { get; }

        public TokenType Type { get; }

        public TokenHints Hints { get; set; }

        public string Content { get; }

        public int Start
        {
            get => Line.Start + ColumnStart;
        }

        public int End
        {
            get => Line.Start + ColumnEnd;
        }

        public Token(PageLine line, RawToken token)
        {
            Page = line.Page;
            Line = line;

            ColumnStart = token.ColumnStart;
            ColumnEnd = token.ColumnEnd;

            Type = token.Type;
            Hints = TokenHints.None;
            Content = token.Content;
        }

        public string ToShortString()
        {
            return $"[{Type}:{Content}]";
        }

        public override string ToString()
        {
            return $"[{Type}:{Content}]{{{Start}:{End}}}";
        }
    }
}
