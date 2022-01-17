using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using NSprak.Expressions;

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

        public int Index { get; }

        public int ColumnStart { get; }

        public int ColumnEnd { get; }

        public TokenType Type { get; }

        public TokenHints Hints { get; set; }

        public string Content { get; }

        public int Start => Line.Start + ColumnStart;

        public int End => Line.Start + ColumnEnd;

        public Expression ExpressionHint { get; set; }

        public Token(PageLine line, int index, RawToken token)
        {
            Page = line.Page;
            Line = line;
            Index = index;

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

        public Token FindNextToken(Predicate<Token> predicate)
        {
            Token result = FindNextToken();
            while (result != null && !predicate(result))
                result = result.FindNextToken();

            return result;
        }

        public Token FindNextToken()
        {
            if (Index + 1 < Line.TokenCount)
                return Line[Index + 1];

            PageLine line = Line.GetNextLine();
            while (line != null)
                if (line.TokenCount > 0)
                    return line[0];
                else
                    line = line.GetNextLine();

            return null;
        }

        public Token FindPreviousToken(Predicate<Token> predicate)
        {
            Token result = FindPreviousToken();
            while (result != null && !predicate(result))
                result = result.FindPreviousToken();

            return result;
        }

        public Token FindPreviousToken()
        {
            if (Index - 1 > 0)
                return Line[Index - 1];

            PageLine line = Line.GetPreviousLine();
            while (line != null)
                if (line.TokenCount > 0)
                    return line[line.TokenCount -1];
                else
                    line = line.GetPreviousLine();

            return null;
        }

        public bool IsInitial()
        {
            Token current = FindPreviousToken();
            while (current != null)
                if (current.Type != TokenType.Comment)
                    return false;
                else
                    current = current.FindPreviousToken();

            return true;
        }
    }
}
