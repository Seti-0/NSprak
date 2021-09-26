using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Tokens;
using NSprak.Exceptions;
using System.Text.RegularExpressions;
using NSprak.Language;

namespace NSprak.Expressions.Patterns
{
    public class MatchIterator
    {
        public bool HasCurrent => Index >= 0 && Index < Items.Count;

        public bool HasNext => Index >= -1 && Index < Items.Count - 1;

        public object Current => Items[Index];

        public List<object> Items { get; set; }
        public int Index { get; set; } = -1;

        public MatchIterator(List<object> items)
        {
            Items = items;
        }

        public override string ToString()
        {
            string names = string.Join(",", Items.Select(x => x.ToString()));
            return $"Index {Index} of [{names}]";
        }

        public bool MoveNext()
        {
            if (Index <= Items.Count - 1)
                Index++;

            return HasCurrent;
        }

        public bool PeekNext(out object next)
        {
            if (!HasNext)
            {
                next = null;
                return false;
            }

            next = Items[Index + 1];
            return true;
        }

        public void Reset()
        {
            Index = -1;
        }

        public MatchException UnexpectedEnd()
        {
            if (HasCurrent)
                return new MatchException($"Found unpexpected item {Current} at index {Index}");

            if (Index == -1)
            {
                PeekNext(out object next);
                return new MatchException($"Found unpexpected item {next} at index {Index}");
            }

            else return new MatchException($"Unexpected end of iteratorof length {Items.Count}");
        }

        public void Assert<T>(out T item)
        {
            if (!MoveNext())
                throw new MatchException($"Expected type {typeof(T).Name}, " +
                    $"no more elements found");

            if (Current is T result)
                item = result;

            else throw new MatchException($"Expected type {typeof(T).Name}, " +
                $"found {Current.GetType().Name}");
        }

        public void AssertToken(out Token current)
        {
            Assert(out current);
        }

        public void AssertTokenType(TokenType type, out Token current)
        {
            Assert(out current);

            if (current.Type != type)
                throw new MatchException($"Expected token type {type}, found {current}");
        }

        public void AssertExpression(out Expression expression)
        {
            Assert(out expression);
        }

        public void AssertKeyword(string keyword)
        {
            AssertKeyword(keyword, out _);
        }

        public void AssertKeyword(string keyword, out Token token)
        {
            Assert(out token);
            token.AssertKeyword(keyword);
        }

        public void AssertKeySymbol(char symbol, out Token token)
        {
            Assert(out token);
            token.AssertKeySymbol(symbol);
        }

        public void AssertType(out SprakType type)
        {
            Assert(out Token current);
            type = current.AssertType();
        }

        public void AssertName(out string name)
        {
            Assert(out Token current);
            name = current.AssertName();
        }

        public void AssertOperator(out Operator op)
        {
            Assert(out Token current);
            op = current.AssertOperator();
        }

        public bool Next<T>(out T item)
        {
            item = default;

            if (PeekNext(out object next))
            {
                if (next is T result)
                {
                    item = result;
                    return true;
                }
            }

            return false;
        }

        public bool NextIsExpression(out Expression expression)
        {
            if (Next(out expression))
            {
                MoveNext();
                return true;
            }

            return false;
        }

        public bool NextIsToken(TokenType type, out Token token)
        {
            if (Next(out token) && token.Type == type)
            {
                MoveNext();
                return true;
            }

            return false;
        }

        public bool NextIsKeyword(string keyword)
        {
            return NextIsKeyword(keyword, out _);
        }

        public bool NextIsKeyword(string keyword, out Token token)
        {
            if (Next(out token) && token.IsKeyWord(keyword))
            {
                MoveNext();
                return true;
            }

            return false;
        }

        public bool NextIsKeySymbol(char keySymbol)
        {
            return NextIsKeySymbol(keySymbol, out _);
        }

        public bool NextIsKeySymbol(char keySymbol, out Token token)
        {
            if (Next(out token) && token.IsKeySymbol(keySymbol))
            {
                MoveNext();
                return true;
            }

            return false;
        }

        public bool NextIsType(out SprakType type)
        {
            if (Next(out Token token) && token.Type == TokenType.Type)
            {
                type = token.AssertType();
                MoveNext();
                return true;
            }

            type = null;
            return false;
        }

        public bool NextIsOperator(out Operator op)
        {
            if (Next(out Token token) && token.Type == TokenType.Operator)
            {
                op = token.AssertOperator();
                MoveNext();
                return true;
            }

            op = null;
            return false;
        }

        public bool AtEnd()
        {
            return !HasNext;
        }

        public void AssertNext()
        {
            if (!HasNext)
                throw new MatchException("Expected something, found end of pattern");
        }

        public void AssertEnd()
        {
            if (HasNext)
            {
                PeekNext(out object next);
                throw new MatchException($"Expected end of pattern, found {next}");
            }
        }
    }
}
