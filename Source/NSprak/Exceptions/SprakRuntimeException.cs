using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Messaging;

namespace NSprak.Exceptions
{
    public class SprakRuntimeException : Exception
    {
        public MessageTemplate Template { get; }

        public IList<object> Args { get; }

        public SprakRuntimeException(MessageTemplate template, params object[] args) 
            : base(template.Render(args))
        {
            Template = template;
            Args = args;
        }

        public SprakRuntimeException(Exception inner, MessageTemplate template, params object[] args) 
            : base(template.Render(args), inner) 
        {
            Template = template;
            Args = args;
        }
    }
}
