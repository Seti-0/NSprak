using NSprak.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class MessageCollection : IEnumerable<Message>
    {
        public bool HasErrors { get; private set; }

        private List<Message> _messages = new List<Message>();

        public IReadOnlyList<Message> Messages => _messages;

        public void Add(Message message)
        {
            HasErrors |= message.IsError;
            _messages.Add(message);
        }

        public void AddError(Token token, string message)
        {
            Add(new TokenMessage(MessageSeverity.Error, token, message));
        }

        public void AddError(Token start, Token end, string message)
        {
            Add(new TokenRangeMessage(MessageSeverity.Error, start, end, message));
        }

        public void Clear()
        {
            _messages.Clear();
            HasErrors = false;
        }

        public IEnumerator<Message> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _messages.GetEnumerator();
        }
    }
}
