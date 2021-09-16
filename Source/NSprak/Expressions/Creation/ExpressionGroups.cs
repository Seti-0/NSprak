using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Creation
{
    public static class ExpressionGroups
    {
        public static Expression Create(MatchIterator iterator)
        {
            Token opToken;

            if (iterator.NextIsToken(TokenType.Operator, out opToken))
            {
                // assume a left operator for now
                Expression righExpr = Create(iterator);
                return new OperatorCall(opToken, null, righExpr);
            }

            Expression nextExpr;

            if (iterator.NextIsKeySymbol(Symbols.OpenBracket))
            {
                iterator.AssertExpression(out nextExpr);
                iterator.AssertKeySymbol(Symbols.CloseBracket, out _);
            }

            else iterator.AssertExpression(out nextExpr);

            if (iterator.AtEnd())
                return nextExpr;

            if (iterator.Next(out List<CollectedIndex> indices))
            {
                foreach (CollectedIndex index in indices)
                    nextExpr = new Indexer(nextExpr, index.Open, index.Index, index.Close);

                iterator.MoveNext();
            }

            if (iterator.AtEnd())
                return nextExpr;

            iterator.AssertTokenType(TokenType.Operator, out opToken);

            if (iterator.AtEnd())
                // Assume a right operator for now
                return new OperatorCall(opToken, nextExpr, null);

            // Assume a binary operator for now
            Expression remainder = Create(iterator);
            return new OperatorCall(opToken, nextExpr, remainder);
        }
    }
}
