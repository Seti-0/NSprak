using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Exceptions
{
    public class CodeBuildingException : NSprakException
    {
        public CodeBuildingException(string message) : base(message) { }

        public CodeBuildingException(string message, Exception inner) : base(message, inner) { }
    }
}
