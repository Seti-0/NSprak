using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using NSprak.Tokens;

namespace NSprakIDE.Controls.Code
{
    public class TokenSeeker
    {
        private readonly TokenPage _page;

        // Not using this at the moment, it doesn't seem needed
        //private int _lastCharIndex;

        // Current line should be non-negative, or -1 if the enumeration has yet to start
        public int CurrentLine { get; private set; } = -1;

        // Current Column should always be non-negative
        public int CurrentColumn { get; private set; } = 0;

        public Token Current => _page[CurrentLine][CurrentColumn];

        public bool HasCurrent
        {
            get
            {
                if (CurrentLine < 0)
                    return false;

                if (CurrentLine >= _page.LineCount)
                    return false;

                var line = _page[CurrentLine];

                if (CurrentColumn >= line.TokenCount)
                    return false;

                return true;
            }
        }

        public EnumerationState State
        {
            get
            {
                if (CurrentLine < 0)
                    return EnumerationState.BeforeStart;

                else if (CurrentLine >= _page.LineCount)
                    return EnumerationState.AfterEnd;

                else 
                    return EnumerationState.HasCurrent;
            }
        }

        public TokenSeeker(TokenPage page)
        {
            _page = page;
        }

        public void EnsureStarted()
        {
            if (CurrentLine == -1)
                Step();
        }

        public bool SeekLine(int lineIndex)
        {
            CurrentLine = Math.Clamp(lineIndex, 0, _page.LineCount);
            CurrentColumn = 0;
            return lineIndex == CurrentLine;
        }

        private bool Seek(int charIndex, int lineIndex)
        {
            CurrentLine = Math.Clamp(lineIndex, 0, _page.LineCount);
            
            if (CurrentLine != lineIndex)
                return false;

            PageLine line = _page[lineIndex];
            int columnIndex = 0;

            bool found = false;

            while (columnIndex <= line.TokenCount && !found)
            {
                if (line[columnIndex].Start >= charIndex)
                    found = true;

                columnIndex++;
            }

            if (found)
            {
                CurrentLine = lineIndex;
                CurrentColumn = columnIndex;
            }

            return found;
        }

        public bool SeekCharacter(int charIndex)
        {
            PageLine line;
            int lineIndex = 0;

            while (true)
            {
                if (lineIndex > _page.LineCount)
                    return false;

                line = _page[lineIndex];

                if (line.Start <= charIndex && line.End > charIndex)
                    break;

                if (line.End >= charIndex)
                    return false;

                lineIndex++;
            }

            return Seek(charIndex, lineIndex);
        }

        public bool Step()
        {
            if (CurrentLine >= _page.LineCount)
                return false;

            PageLine line;

            if (CurrentLine == -1)
            {
                CurrentLine = 0;
                CurrentColumn = 0;

                line = _page[0];
            }
            else
            {
                line = _page[CurrentLine];

                if (CurrentColumn < line.TokenCount)
                    CurrentColumn++;
            }

            if (CurrentColumn >= line.TokenCount)
            {
                CurrentColumn = 0;

                while (true)
                {
                    CurrentLine++;

                    if (CurrentLine >= _page.LineCount)
                        return false;

                    line = _page[CurrentLine];

                    if (line.TokenCount > 0)
                        break;
                }
            }

            return true;
        }

        public void Reset()
        {
            CurrentLine = -1;
            CurrentColumn = -1;

            //_lastCharIndex = 0;
        }
    }
}
