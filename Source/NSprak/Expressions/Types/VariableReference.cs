using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSprak.Expressions.Types
{
    public class VariableReference : SingleTokenExpression
    {
        public string Name => Token.Content;

        public VariableReference(Token token) : base(token)
        {
            token.AssertType(TokenType.Name);
        }

        public override string ToString()
        {
            return $"(var: {Name})";
        }
    }
}
