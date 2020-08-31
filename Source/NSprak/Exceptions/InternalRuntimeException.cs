using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Exceptions
{
    public class SprakInternalRuntimeException : Exception
    {
        public SprakInternalRuntimeException(string message) : base(message) { }

        public SprakInternalRuntimeException(string message, Exception inner) : base(message, inner) { }
    }
}
