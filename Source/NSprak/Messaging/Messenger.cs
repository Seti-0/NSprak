﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class Messenger
    {
        private readonly List<Message> _messages = new List<Message>();

        public IReadOnlyList<Message> Messages => _messages;

        public bool HasErrors { get; private set; }

        public void Clear()
        {
            _messages.Clear();
            HasErrors = false;
        }

        public void Add(MessageLocation location,
            MessageTemplate message, params object[] parameters)
        {
            _messages.Add(new Message(message, location, parameters));
            HasErrors |= message.IsError;
        }
    }
}
