using System;
using System.Diagnostics.CodeAnalysis;

using NSprak.Tokens;

namespace NSprak.Messaging
{
    public class MessageLocation
    {
        private readonly Token _startToken, _endToken;

        public int Start => _startToken.Start;

        public int End => _endToken.End;

        public int LineStart => _startToken.LineNumber;

        public int LineEnd => _endToken.LineNumber + 1;

        public int ColumnStart => _startToken.ColumnStart;

        public int ColumnEnd => _endToken.ColumnEnd;

        public MessageLocation(Token token)
        {
            _startToken = token;
            _endToken = token;
        }

        public MessageLocation(Token start, Token end)
        {
            _startToken = start;
            _endToken = end;
        }

        public override string ToString()
        {
            string line = LineStart.ToString();
            if (LineEnd - LineStart > 1)
                line += ":" + LineEnd;

            string column = ColumnStart.ToString();
            if (ColumnStart != ColumnEnd)
                line += ":" + ColumnEnd;

            // Confusingly, the column end can come before the column
            // start here, if the location spans multiple lines.
            return $"(line: {line}, col: {column})";
        }
    }
}
