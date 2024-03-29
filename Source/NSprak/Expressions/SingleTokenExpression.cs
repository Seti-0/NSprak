﻿using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSprak.Expressions
{
    public abstract class SingleTokenExpression : Expression
    {
        public Token Token { get; }

        public override Token StartToken => Token;

        public override Token EndToken => Token;

        public SingleTokenExpression(Token token)
        {
            Token = token;
            token.ExpressionHint = this;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return Token;
        }
    }
}
