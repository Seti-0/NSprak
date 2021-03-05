using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;

using NSprak.Messaging;

namespace NSprak.Tokens
{
    public class TokenPage : IEnumerable<PageLine>
    {
        public int CharacterCount { get; private set; }

        public int TokenCount { get; private set; }

        private readonly List<PageLine> _lines = new List<PageLine>();

        public int LineCount => _lines.Count;

        public PageLine this[int lineNumber] => _lines[lineNumber];

        public PageLine this[Index lineIndex] => _lines[lineIndex];

        public void Update(string source, MessageCollection messenger)
        {
            _lines.Clear();

            IList<string> elements;
            IList<int> indices;
            StringHelper.Split(source, '\n', out elements, out indices);

            TokenCount = 0;
            CharacterCount = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                int start = indices[i];
                string line = elements[i];

                int end;

                if (i == indices.Count - 1)
                    end = source.Length;
                else
                    end = indices[i + 1];

                TokenHelper.TryParse(line, out IList<RawToken> tokens);
                PageLine pageLine = new PageLine(this, start, end, tokens, messenger);
                _lines.Add(pageLine);

                CharacterCount = end;
                TokenCount += pageLine.TokenCount;
            }

            // I don't think this is actually needed
            UpdateLineBounds();
        }

        private void UpdateLineBounds()
        {
            int charIndex = 0;

            for (int i = 0; i < _lines.Count; i++)
            {
                int lineIndex = i;
                _lines[i].UpdateLocation(charIndex, lineIndex);
                charIndex += _lines[i].Length;
            }
        }

        public IEnumerator<PageLine> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join("\n", _lines.Select(x => x.ToShortString()));
        }

        public Token GetToken(int offset)
        {
            Token result = null;

            foreach (PageLine line in _lines)
                foreach (Token token in line)
                {
                    result = token;

                    if (token.End > offset)
                        break;
                }

            return result;
        }
    }
}
