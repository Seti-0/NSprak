using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Tokens;

namespace NSprak.Messaging
{
    public class TokenMessage : Message
    {
        public Token Token { get; }

        public override int LineStart => Token != null ? Token.Line.LineNumber : -1;

        public override int LineEnd => Token != null ? Token.Line.LineNumber + 1 : -1;

        public override int Start => Token?.Start ?? -1;

        public override int End => Token?.End ?? -1;

        public TokenMessage(MessageSeverity severity, Token token, string message)
        {
            Token = token;
            UserText = message;
            Severity = severity;
        }
    }
}
