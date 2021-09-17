using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Language;
using NSprak.Tokens;

using NSprak.Expressions.Types;
using NSprak.Expressions.Patterns;

namespace NSprak.Expressions.Creation
{
    public static class Variables
    {
        public static VariableReference CreateReference(MatchIterator iterator)
        {
            iterator.AssertTokenType(TokenType.Name, out Token token);
            iterator.AssertEnd();

            return new VariableReference(token);
        }

        public static VariableAssignment Assignment(MatchIterator iterator)
        {

            if (!iterator.NextIsToken(TokenType.Type, out Token type))
                type = null;

            iterator.AssertTokenType(TokenType.Name, out Token name);
            iterator.AssertTokenType(TokenType.Operator, out Token op);

            if (!iterator.NextIsExpression(out Expression value))
                value = null;

            iterator.AssertEnd();

            return new VariableAssignment(type, name, op, value);
        }
    }
}
