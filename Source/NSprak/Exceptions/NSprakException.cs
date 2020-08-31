using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Exceptions
{
    public class NSprakException : Exception
    {
        public NSprakException(string message) : base(message) { }

        public NSprakException(string message, Exception inner) : base(message, inner) { }
    }
}
