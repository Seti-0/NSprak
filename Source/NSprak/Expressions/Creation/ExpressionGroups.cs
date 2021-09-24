using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            LinkedList<object> items = new LinkedList<object>();

            iterator.AssertNext();

            // postfix unary operators are not supported here
            // (Like i--, etc)
            // I don't think they are really needed for sprak, and defaulting
            // to assuming prefix simplifies things.

            while (true)
            {
                items.AddLast(GetValue(iterator));

                if (iterator.AtEnd())
                    break;

                iterator.AssertTokenType(TokenType.Operator, out Token op);
                items.AddLast(op);

                if (iterator.AtEnd())
                    break;
            }

            void ParseOperatorToken(LinkedListNode<object> opNode)
            {
                Token token = (Token)opNode.Value;
                LinkedListNode<object> leftNode = opNode.Previous;
                LinkedListNode<object> rightNode = opNode.Next;

                Expression left = (Expression)leftNode.Value;
                Expression right = (Expression)rightNode?.Value;
                OperatorCall result = new OperatorCall(token, left, right);

                items.AddBefore(leftNode, result);

                items.Remove(leftNode);
                items.Remove(opNode);

                if (rightNode != null)
                    items.Remove(rightNode);
            }

            LinkedListNode<object> current, next;

            foreach (Operator[] group in Operator.OperatorPrecedenceGroups)
            {
                current = items.First.Next;

                while (current != null)
                {
                    next = current.Next?.Next;

                    Token token = (Token)current.Value;
                    if (Operator.TryParse(out Operator op, text:token.Content))
                        if (group.Contains(op))
                            ParseOperatorToken(current);

                    current = next;
                }                
            }

            current = items.First.Next;
            while (current != null)
            {
                ParseOperatorToken(current);
                current = current.Next?.Next;
            }

            if (items.First.Next != null)
                throw new Exception("Unable to parse all operators");

            return (Expression)items.First.Value;
        }

        private static Expression GetValue(MatchIterator iterator)
        {
            if (iterator.NextIsToken(TokenType.Operator, out Token opToken))
                // Early out: if we are starting with an operator, than it is
                // a right unary operator, and unary ops have the highest 
                // precedence at the moment.
                return new OperatorCall(opToken, null, GetValue(iterator));

            Expression expression;

            // We don't have to recursively descend through the brackets - 
            // that is handled at the pattern matching level. As far as we are
            // concerned, there are either expressions or expressions wrapped 
            // in brackets.

            if (iterator.NextIsKeySymbol(Symbols.OpenBracket))
            {
                iterator.AssertExpression(out expression);
                iterator.AssertKeySymbol(Symbols.CloseBracket, out _);
            }
            else iterator.AssertExpression(out expression);

            // Finally, consider any indexing that follow the expression.
            // Sprak only allows one level of this, but that will be enforced
            // later where a more friendly error can be displayed.

            if (iterator.Next(out List<CollectedIndex> indices))
            {
                foreach (CollectedIndex index in indices)
                    expression = new Indexer(expression, index.Open, index.Index, index.Close);

                iterator.MoveNext();
            }

            return expression;
        }

        public static Expression CreateOld(MatchIterator iterator)
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
