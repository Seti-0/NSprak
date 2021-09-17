using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Messaging;

namespace NSprak.Expressions.Structure
{
    public class StatementEnumerator
    {
        private readonly Stack<List<Expression>> _collections = new Stack<List<Expression>>();

        private readonly List<Expression> _expressions;

        public int Index { get; private set; } = -1;

        public Expression Current => _expressions[Index];

        public bool HasCurrent => Index >= 0 && Index < _expressions.Count;

        public bool HasNext => Index >= -1 && Index < _expressions.Count - 1;

        public StatementEnumerator(IEnumerable<Expression> expressions)
        {
            _expressions = expressions.ToList();
        }

        public bool MoveNext(bool collect = true)
        {
            if (Index >= _expressions.Count)
                return false;

            if (Index >= 0 && collect)
                if (_collections.Any())
                    _collections.Peek().Add(Current);

            Index++;
            return Index < _expressions.Count;
        }

        public void Complete()
        {
            while (MoveNext()) ;
        }

        public void Reset()
        {
            Index = -1;
            _collections.Clear();
        }

        public void BeginCollection()
        {
            _collections.Push(new List<Expression>());
        }

        public List<Expression> EndCollection()
        {
            if (_collections.Count == 0)
                throw new InvalidOperationException("No current collections to end");

            return _collections.Pop();
        }

        public void AddToCollection(Expression expression)
        {
            if (_collections.Count == 0)
                throw new InvalidOperationException("No current collection to add to");

            _collections.Peek().Add(expression);
        }

        public bool SeekHeader(out Header header)
        {
            if (Index == -1)
                MoveNext();

            while (HasCurrent)
            {
                if (Current is Header result)
                {
                    header = result;
                    return true;
                }

                MoveNext();
            }

            header = null;
            return false;
        }

        /*
        public bool SeekEnd(
            Header initial, out Expression endStatement, Messenger messenger)
        {
            if (Index == -1)
                MoveNext();

            Stack<Header> stack = new Stack<Header>();

            while (HasCurrent)
            {
                if (Current is ElseHeader || Current is ElseIfHeader)
                {
                    if (stack.Count == 0)
                    {
                        endStatement = Current;
                        return true;
                    }

                    Header previous = stack.Peek();
                    if (previous is IfHeader || previous is ElseIfHeader)
                        stack.Pop();
                }
                else if (Current is Header header)
                    stack.Push(header);

                else if (Current is Command command && command.Keyword == Keywords.End)
                {
                    if (stack.Count == 0)
                    {
                        endStatement = command;
                        return true;
                    }

                    stack.Pop();
                }

                MoveNext();
            }
        }
        */

        public bool SeekEnd(out Token endToken)
        {
            if (Index == -1)
                MoveNext();

            int depth = 1;

            while (HasCurrent)
            {
                if (Current is Header)
                    depth++;

                else if (Current is Command command && command.Keyword == Keywords.End)
                {
                    depth--;
                    if (depth == 0)
                    {
                        endToken = command.Token;
                        return true;
                    }
                }

                MoveNext();
            }

            endToken = null;
            return false;
        }

        public override string ToString()
        {
            string result = $"[{Index}]";

            if (HasCurrent)
                result += Current.ToString();

            else if (HasNext)
                result += " (start)";

            return result;
        }
    }
}
