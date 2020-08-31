using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Tokens;

namespace NSprak.Exceptions
{
    public class TokenCheckException : NSprakException
    {
        public Token ErrorToken { get; }

        public TokenCheckException(Token token, string message) : base(message)
        {
            ErrorToken = token;
        }

        public TokenCheckException(Token token, string message, Exception inner) : base(message, inner)
        {
            ErrorToken = token;
        }
    }
}
