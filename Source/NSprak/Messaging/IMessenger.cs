using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public interface IMessenger
    {
        public bool HasErrors { get; }

        public void Clear();

        public void Add(MessageLocation location,
            Message message, params object[] parameters);
    }
}
