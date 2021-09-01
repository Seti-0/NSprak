using System;
using System.Diagnostics.CodeAnalysis;

using NSprak.Tokens;

namespace NSprak.Messaging
{
    public class MessageLocation : IEquatable<MessageLocation>
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

        public bool Equals([AllowNull] MessageLocation other)
            => other != null
            && other.Start == Start
            && other.End == End
            && other.LineStart == LineStart
            && other.LineEnd == LineEnd;

        public override bool Equals(object obj)
            => obj is MessageLocation msg && Equals(msg);

        public static bool operator ==(MessageLocation a, MessageLocation b)
            => ((a is null) && (b is null))
            || ((!(a is null)) && a.Equals(b));

        public static bool operator !=(MessageLocation a, MessageLocation b)
            => (a is null && !(b is null))
            || ((!(a is null)) && (!a.Equals(b)));

        public override int GetHashCode()
            => HashCode.Combine(Start, End, LineStart, LineEnd);

        public override string ToString()
        {
            string line = LineStart.ToString();
            if (LineEnd - LineStart > 1)
                line += ":" + LineEnd;

            string column = Start.ToString();
            if (End - Start > 1)
                line += ":" + End;

            return $"(line: {line}, col: {column})";
        }
    }
}
