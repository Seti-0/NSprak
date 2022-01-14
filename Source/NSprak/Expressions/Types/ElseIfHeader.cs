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
    public class ElseIfHeader : Header, IConditionalSubComponent
    {
        public override string FriendlyBlockName => "else if";

        public Expression Condition { get; }

        public Token ElseToken { get; }

        public Token IfToken { get; }

        public override Token StartToken => ElseToken;

        public override Token EndToken => Condition.EndToken;

        public string EndLabelHint { get; set; }

        public IConditionalSubComponent NextConditionalComponentHint { get; set; }

        public ElseIfHeader(
            Token elseToken, Token ifToken, Expression condition)
        {
            elseToken.AssertKeyword(Keywords.Else);
            ElseToken = elseToken;
            elseToken.ExpressionHint = this;

            ifToken.AssertKeyword(Keywords.If);
            IfToken = ifToken;
            elseToken.ExpressionHint = this;

            Condition = condition;
        }

        public override string ToString()
        {
            return $"Else If {Condition}";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return new Expression[] { Condition };
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return ElseToken;
            yield return IfToken;
        }
    }
}
