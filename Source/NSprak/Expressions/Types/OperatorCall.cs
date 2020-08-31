using System;
using System.Collections.Generic;

using NSprak.Language;
using NSprak.Language.Builtins;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class OperatorCall : Expression
    {
        public Operator Operator { get; }

        public Expression LeftInput { get; }

        public Expression RightInput { get; }

        public BuiltInFunction BuiltInFunctionHint { get; set; }

        public Token OperatorToken { get; }

        public override Token StartToken => LeftInput?.StartToken ?? OperatorToken;

        public override Token EndToken => RightInput?.EndToken ?? OperatorToken;

        public OperatorCall(Token opToken, Expression left, Expression right)
        {
            Operator = opToken.AssertOperator();
            OperatorToken = opToken;

            LeftInput = left;
            RightInput = right;
        }

        public override string ToString()
        {
            string result = Operator.Text;

            if (LeftInput != null)
                result = $"{LeftInput} {result}";

            if (RightInput != null)
                result = $"{result} {RightInput}";

            return result;
        }

        /*
        public override void Check(MessageCollection messenger)
        {
            base.Check(messenger);

            if (Operator.IsAssignment)
                RaiseError(messenger, $"{Operator.Text} is an assignment operator, and cannot be used in any other context");

            else
            {
                string needs = null;
                string cant = null;
                string type;

                bool left = LeftInput != null;
                bool right = RightInput != null;

                switch (Operator.Side)
                {
                    case OperatorSide.Both:
                        type = "binary";
                        if (!(left && right)) needs = "on either side";
                        break;

                    case OperatorSide.Left:
                        type = "left";
                        if (!left) needs = "on the left side";
                        if (right) cant = "on the right side";
                        break;

                    case OperatorSide.Right:
                        type = "right";
                        if (left) cant = "on the left side";
                        if (!right) needs = "on the right side";
                        break;

                    default: throw new NotImplementedException();
                }

                if (needs != null)
                    RaiseError(messenger, $"{Operator.Text} is a {type} operator, and needs something {needs}");

                if (cant != null)
                    RaiseError(messenger, $"{Operator.Text} is a {type} operator, and can't have something {cant}");
            }
        }
        */

        public override IEnumerable<Expression> GetSubExpressions()
        {
            if (LeftInput != null)
                yield return LeftInput;

            if (RightInput != null)
                yield return RightInput;
        }
    }
}
