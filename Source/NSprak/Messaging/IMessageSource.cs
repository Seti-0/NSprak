using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public interface IMessageSource
    {
        public MessageCollection Messages { get; }
    }
}
