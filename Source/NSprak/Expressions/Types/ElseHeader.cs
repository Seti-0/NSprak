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
    public class ElseHeader : Header, IConditionalSubComponent
    {
        public override string FriendlyBlockName => "else";

        public Token ElseToken { get; }

        public override Token StartToken => ElseToken;

        public override Token EndToken => ElseToken;

        public string EndLabelHint { get; set; }

        IConditionalSubComponent 
            IConditionalSubComponent.NextConditionalComponentHint => null;

        public ElseHeader(Token elseToken)
        {
            elseToken.AssertKeyword(Keywords.Else);
            ElseToken = elseToken;
            elseToken.ExpressionHint = this;
        }

        public override string ToString()
        {
            return $"Else";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return ElseToken;
        }
    }
}
