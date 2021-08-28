using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class DummyMessenger : Messenger
    {
        public static readonly DummyMessenger Instance = new DummyMessenger();

        public bool HasErrors { get; private set; }

        private DummyMessenger() { }

        public void Clear()
        {
            HasErrors = false;
        }

        public void Add(MessageLocation location,
            MessageTemplate message, params object[] parameters) 
        {
            HasErrors |= message.Severity == MessageSeverity.Error;
        }
    }
}
