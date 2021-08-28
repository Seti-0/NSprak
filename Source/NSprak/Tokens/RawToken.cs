using NSprak.Messaging;

namespace NSprak.Tokens
{
    public class RawToken
    {
        public TokenType Type = TokenType.Unknown;
        public string Content;

        public int ColumnStart; // Inclusive
        public int ColumnEnd;  // Exclusive

        public bool Error;
        public MessageTemplate ErrorMessage;
        public object[] ErrorParams;
    }
}
