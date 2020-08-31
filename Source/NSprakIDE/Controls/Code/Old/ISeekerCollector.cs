using System;
using System.Collections.Generic;
using System.Text;

namespace NSprakIDE.Controls.Code
{
    public interface ISeekerCollector<T>
    {
        public EnumerationState State { get; }

        public TextAssociation<T> Current { get; }

        public int CharacterStart { get; }

        public int CharacterEnd { get; }

        public int LineStart { get; }

        public int LineEnd { get; }

        public void Reset();

        public bool SeekLine(int lineIndex);

        public bool Step();
    }

    public class SimpleExpressionSeeker : ISeekerCollector<Expression>
    {
        private Expression _current;

        public EnumerationState State { get; private set; }

        public Expression Current
        {
            get
            {
                if (State == EnumerationState.HasCurrent)
                    return _current;

                throw new InvalidOperationException("Enumerator does not have a current item");
            }
        }

        public bool HasCurrent => State == EnumerationState.HasCurrent;

        public int CharacterStart => _current.StartToken.Start;

        public int CharacterEnd => _current.EndToken.End;

        public SimpleExpressionSeeker(Expression expression)
        {
            _current = expression;
        }

        public void Reset()
        {
            State = EnumerationState.BeforeStart;
        }

        public bool Step()
        {
            switch (State)
            {
                case EnumerationState.BeforeStart: State = EnumerationState.HasCurrent; return true;
                case EnumerationState.HasCurrent: State = EnumerationState.AfterEnd; return false;
                default: return false;
            }
        }

        public bool SeekLine(int target)
        {
            int startLine = _current.StartToken.Line.LineNumber;
            int endLine = _current.EndToken.Line.LineNumber;

            if (target < startLine)
            {
                State = EnumerationState.BeforeStart;
                return false;
            }

            if (target > endLine)
            {
                State = EnumerationState.AfterEnd;
                return false;
            }

            State = EnumerationState.HasCurrent;
            return true;
        }

        public bool SeekCharacter(int target)
        {
            int startChar = _current.StartToken.Start;
            int endChar = _current.EndToken.End;

            if (target < startChar)
            {
                State = EnumerationState.BeforeStart;
                return false;
            }

            if (target > endChar)
            {
                State = EnumerationState.AfterEnd;
                return false;
            }

            State = EnumerationState.HasCurrent;
            return true;
        }
    }
}
