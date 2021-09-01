using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public class PatternEnumerator
    {
        private readonly List<Token> _elements = new List<Token>();

        public int Index { get; set; } = -1;

        public bool HasPrevious => Index >= 1 && Index < _elements.Count + 1;

        public bool HasCurrent => Index >= 0 && Index < _elements.Count;

        public bool HasNext => Index >= -1 && Index < _elements.Count - 1;

        public Token Previous => _elements[Index - 1];

        public Token Current => _elements[Index];

        public PatternEnumerator(IEnumerable<Token> tokens)
        {
            _elements.AddRange(tokens);
        }

        public bool MoveNext()
        {
            if (Index < _elements.Count + 1)
            {
                Index++;

                // Not graceful, but it's easiest to ignore these altogether
                while (HasCurrent && Current.Type == TokenType.Comment)
                    Index++;
            }

            return HasCurrent;
        }

        public override string ToString()
        {
            string tokens = string.Join("", _elements.Select(x => x.ToString()));
            return $"Index {Index} of {tokens}";
        }
    }
}
