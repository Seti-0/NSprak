using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NSprak.Operations.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Operations;

namespace NSprak.Expressions.Types
{
    public class IfHeader : Header
    {
        public override string FriendlyBlockName => "if";

        public Expression Condition { get; }

        public Token IfToken { get; }

        public override Token StartToken => IfToken;

        public override Token EndToken => Condition.EndToken;

        public IConditionalSubComponent NextConditionalComponentHint { get; set; }

        public IfHeader(Token ifToken, Expression condition)
        {
            ifToken.AssertKeyword(Keywords.If);
            IfToken = ifToken;
            ifToken.ExpressionHint = this;

            Condition = condition;
        }

        public override string ToString()
        {
            return $"If {Condition}";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return new Expression[] { Condition };
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return IfToken;
        }
    }
}
