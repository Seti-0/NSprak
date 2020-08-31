using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Exceptions
{
    public class SprakInternalExecutionException : Exception
    {
        public SprakInternalExecutionException(string message) : base(message) { }

        public SprakInternalExecutionException(string message, Exception inner) : base(message, inner) { }
    }
}
