using NSprak.Tokens;

namespace NSprak.Messaging
{
    public class MessageLocation
    {
        public int LineStart { get; }

        public int LineEnd { get; }

        public int Start { get; }

        public int End { get; }

        public MessageLocation(Token token)
        {
            Start = token.Start;
            End = token.End;
            LineStart = token.LineNumber;
            LineEnd = token.LineNumber + 1;
        }

        public MessageLocation(Token start, Token end)
        {
            Start = start.Start;
            End = end.End;
            LineStart = start.LineNumber;
            LineEnd = end.LineNumber + 1;
        }

        public MessageLocation(int start, int end, int lineStart, int lineEnd)
        {
            Start = start;
            End = end;
            LineStart = lineStart;
            LineEnd = lineEnd;
        }
    }
}
