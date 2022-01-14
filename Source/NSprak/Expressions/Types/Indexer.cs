using NSprak.Language;
using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Types
{
    public class Indexer : Expression
    {
        public Expression SourceExpression { get; }

        public Token OpeningBracket { get; }

        public Expression IndexExpression { get; }

        public Token ClosingBracket { get; }

        public override Token StartToken => SourceExpression.StartToken;

        public override Token EndToken => ClosingBracket;

        public Indexer(Expression source, Token opener, Expression index, Token closer)
        {
            opener.AssertKeySymbol(Symbols.OpenSquareBracket);
            closer.AssertKeySymbol(Symbols.CloseSquareBracket);

            SourceExpression = source;
            OpeningBracket = opener;
            IndexExpression = index;
            ClosingBracket = closer;

            opener.ExpressionHint = this;
            closer.ExpressionHint = this;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return new Expression[] { SourceExpression, IndexExpression };
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return OpeningBracket;
            yield return ClosingBracket;
        }
    }
}
