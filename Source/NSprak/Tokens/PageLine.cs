﻿using NSprak.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Tokens
{
    public class PageLine : IEnumerable<Token>
    {
        public TokenPage Page { get; }

        public int LineNumber { get; private set; }

        private readonly IList<Token> _tokens = null;

        public int Start { get; private set; }

        public int Length { get; }

        public int TokenCount
        {
            get => _tokens.Count;
        }

        public int End
        {
            get => Start + Length;
        }


        public Token this[int index]
        {
            get => _tokens[index];
        }

        public PageLine(TokenPage page, int location, int lineNumber)
        {
            Page = page;

            _tokens = new Token[0];

            Start = location;
            Length = 0;
            LineNumber = lineNumber;
        }

        public PageLine(TokenPage page, int lineNumber, int lineStart, int lineEnd, IList<RawToken> tokens, Messenger messenger)
        {
            Page = page;

            _tokens = new List<Token>(tokens.Count);
            int i = 0;
            foreach (RawToken raw in tokens)
            {
                Token token = new Token(this, i, raw);
                
                if (raw.Error)
                    messenger.AtToken(token, raw.ErrorMessage, raw.ErrorParams);

                _tokens.Add(token);
                
                i++;
            }

            Start = lineStart;
            Length = lineEnd - lineStart;
            LineNumber = lineNumber;
        }

        public void UpdateLocation(int start, int lineNumber)
        {
            Start = start;
            LineNumber = lineNumber;
        }

        public string ToShortString()
        {
            string tokens = string.Join("", _tokens.Select(x => x.ToShortString()));
            return tokens;
        }

        public override string ToString()
        {
            string tokens = string.Join("", _tokens.Select(x => x.ToShortString()));
            return $"{tokens}-{{{Start}:{End}}}";
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return _tokens.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        public PageLine GetNextLine()
        {
            if (LineNumber + 1 < Page.LineCount)
                return Page[LineNumber + 1];

            return null;
        }

        public PageLine GetPreviousLine()
        {
            if (LineNumber - 1 > 0)
                return Page[LineNumber - 1];

            return null;
        }
    }
}
