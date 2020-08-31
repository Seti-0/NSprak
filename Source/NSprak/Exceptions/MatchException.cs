using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Exceptions
{
    public class MatchException : NSprakException
    {
        public MatchException(string message) : base(message) { }

        public MatchException(string message, Exception inner) : base(message, inner) { }
    }
}
