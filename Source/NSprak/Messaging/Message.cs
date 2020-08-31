using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public enum MessageSeverity
    {
        Error, Warning, Message
    }

    public abstract class Message
    {
        public MessageSeverity Severity { get; protected set; }

        public bool IsError => Severity == MessageSeverity.Error;

        public string UserText { get; protected set; }

        public abstract int LineStart { get; }

        public abstract int LineEnd { get; }

        public abstract int Start { get; }

        public abstract int End { get; }

        public object DebugObject { get; set; }

        public override string ToString()
        {
            string error = IsError ? "[Error]" : "";
            string result = $"({LineStart}){error} {UserText}";

            return result;
        }
    }
}
