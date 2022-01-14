using System;
using System.Collections.Generic;
using NSprak.Functions;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class OperatorCall : Expression
    {
        public string OperatorText { get; }

        public Expression LeftInput { get; }

        public Expression RightInput { get; }

        public BuiltInFunction BuiltInFunctionHint { get; set; }

        public Token OperatorToken { get; }

        public override Token StartToken => LeftInput?.StartToken ?? OperatorToken;

        public override Token EndToken => RightInput?.EndToken ?? OperatorToken;

        public OperatorCall(Token opToken, Expression left, Expression right)
        {
            OperatorText = opToken.Content;
            OperatorToken = opToken;
            opToken.ExpressionHint = this;

            LeftInput = left;
            RightInput = right;
        }

        public override string ToString()
        {
            string result = OperatorText;

            if (LeftInput != null)
                result = $"{LeftInput} {result}";

            if (RightInput != null)
                result = $"{result} {RightInput}";

            return result;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            if (LeftInput != null)
                yield return LeftInput;

            if (RightInput != null)
                yield return RightInput;
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return OperatorToken;
        }
    }
}
