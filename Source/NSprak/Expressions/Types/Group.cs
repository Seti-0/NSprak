﻿using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Types
{
    public class Group : Expression
    {
        public Token OpenBracket { get; }

        public Expression Value { get; }

        public Token CloseBracket { get; }

        public override Token StartToken => OpenBracket;

        public override Token EndToken => CloseBracket;

        public Group(Token open, Expression value, Token close)
        {
            OpenBracket = open;
            Value = value;
            CloseBracket = close;

            open.ExpressionHint = this;
            close.ExpressionHint = this;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return new Expression[] { Value };
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return OpenBracket;
            yield return CloseBracket;
        }
    }
}
