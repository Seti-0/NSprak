using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public class Message
    {
        public MessageTemplate Template { get; }

        public MessageLocation Location { get; }

        public bool AccountedFor { get; set; }

        public object[] Parameters { get; }

        public string RenderedText
        {
            get => Template.Render(Parameters);
        }

        public Message(MessageTemplate template, 
            MessageLocation location, params object[] parameters)
        {
            Template = template;
            Location = location;
            Parameters = parameters;
        }
    }
}
