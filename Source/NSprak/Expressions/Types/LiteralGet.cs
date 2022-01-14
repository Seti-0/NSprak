using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Exceptions;
using NSprak.Operations.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Language.Values;
using NSprak.Tokens;
using NSprak.Operations;

namespace NSprak.Expressions.Types
{
    public class LiteralGet : SingleTokenExpression
    {
        public Value Value { get; }

        public LiteralGet(Token token) : base(token)
        {
            switch (Token.Type)
            {
                case TokenType.Boolean:

                    if (bool.TryParse(token.Content, out bool boolean))
                        Value = new SprakBoolean(boolean);

                    else throw new TokenCheckException(token, $"Unable to parse boolean from {token}");

                    break;

                case TokenType.Number:

                    if (double.TryParse(token.Content, out double number))
                        Value = new SprakNumber(number);

                    else throw new TokenCheckException(token, $"Unable to parse number from {token}");

                    break;

                case TokenType.String:

                    Value = new SprakString(token.Content);

                    break;

                default: throw new TokenCheckException(
                    token, $"Unsupported token for {nameof(LiteralGet)}: {token}");
            }

            token.ExpressionHint = this;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "<null>";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }
    }
}
