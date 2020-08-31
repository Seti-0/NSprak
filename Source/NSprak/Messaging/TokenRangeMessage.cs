using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class TokenRangeMessage : Message
    {
        public Token StartToken { get; }

        public Token EndToken { get; }

        public override int LineStart => StartToken != null ? StartToken.Line.LineNumber : -1;

        public override int LineEnd => EndToken != null ? EndToken.Line.LineNumber + 1 : -1;

        public override int Start => StartToken?.Start ?? -1;

        public override int End => EndToken?.End ?? -1;

        public TokenRangeMessage(MessageSeverity severity, Token start, Token end, string message)
        {
            UserText = message;
            Severity = severity;
            StartToken = start;
            EndToken = end;
        }
    }
}
