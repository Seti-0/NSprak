using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSprak.Operations.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Operations;

namespace NSprak.Expressions.Types
{
    public class LiteralArrayGet: Expression
    {
        public List<Expression> Elements { get; }

        public override Token StartToken { get; }

        public override Token EndToken { get; }

        public LiteralArrayGet(Token startToken, Token endToken, List<Expression> elements)
        {
            startToken.AssertKeySymbol(Symbols.OpenSquareBracket);
            endToken.AssertKeySymbol(Symbols.CloseSquareBracket);

            StartToken = startToken;
            EndToken = endToken;
            Elements = elements;

            startToken.ExpressionHint = this;
            endToken.ExpressionHint = this;
        }

        public override string ToString()
        {
            string elements = string.Join(",", Elements.Select(x => x.ToString()));
            return $"[{elements}]";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Elements;
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return StartToken;
            yield return EndToken;
        }
    }
}
