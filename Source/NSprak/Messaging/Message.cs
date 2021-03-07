using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class Message
    {
        public MessageSeverity Severity { get; set; }

        public bool IsError => Severity == MessageSeverity.Error;

        public string Summary { get; set; }

        public override string ToString()
        {
            string severity = Enum.GetName(typeof(MessageSeverity), Severity);
            string result = $"[{severity}] {Summary}";

            return result;
        }
    }
}
