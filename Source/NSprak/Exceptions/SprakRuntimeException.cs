using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Exceptions
{
    public class SprakRuntimeException : Exception
    {
        public SprakRuntimeException(string message) : base(message) { }

        public SprakRuntimeException(string message, Exception inner) : base(message, inner) { }
    }
}
