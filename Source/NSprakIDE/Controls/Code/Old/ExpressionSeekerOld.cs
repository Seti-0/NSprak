using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows.Shapes;
using NSprak.Expressions;
using NSprak.Expressions.Patterns;

namespace NSprakIDE.Controls.Code
{
    public class TextAssociation
    {
        public int Start { get; }

        public int End { get; }

        public Expression Content { get; }

        public TextAssociation(int charStart, int charEnd, Expression content)
        {
            Start = charStart;
            End = charEnd;

            Content = content;
        }
    }

    public class ExpressionSeekerOld
    {
        private Expression _parent;
        private List<ExpressionSeekerOld> _children;
        private int _index;
        private bool _inParent;

        public int CharacterStart => _parent.StartToken.Start;

        public int CharacterEnd => _parent.EndToken.End;

        public int LineStart => _parent.StartToken.LineNumber;

        public int LineEnd => _parent.EndToken.LineNumber;

        public TextAssociation Current { get; set; }

        public bool HasCurrent => _inParent || (_index >= 0 && _index < _children.Count);
        public EnumerationState State
        {
            get
            {
                if (_inParent)
                    return EnumerationState.HasCurrent;

                if (_index < 0)
                    return EnumerationState.BeforeStart;

                if (_index >= _children.Count)
                    return EnumerationState.AfterEnd;

                return EnumerationState.HasCurrent;
            }
        }

        public ExpressionSeekerOld(Expression parent, IEnumerable<Expression> children)
        {
            _parent = parent;
            
            foreach (Expression child in children)
            {
                IEnumerable<Expression> grandChildren = child.GetSubExpressions();
                _children.Add(new ExpressionSeekerOld(child, grandChildren));
            }

            Reset();
        }

        public void Reset()
        {
            _index = -1;
            _inParent = false;

            Current = null;
        }

        public bool SeekLine(int lineIndex)
        {
            int parentStart = _parent.StartToken.LineNumber;
            int parentEnd = _parent.EndToken.LineNumber;

            if (lineIndex < parentStart)
            {
                _index = -1;
                _inParent = false;
                return false;
            }

            else if (lineIndex >= parentEnd)
            {
                _index = _children.Count;
                _inParent = false;
                return false;
            }

            else if (_children.Count == 0 || lineIndex < _children[0].LineStart)
            {
                _index = -1;
                _inParent = true;
                return true;
            }

            
            else if (_children.Count != 0 && lineIndex >= _children[^1].LineEnd)
            {
                _index = _children.Count;
                _inParent = true;
                return true;
            }

            for (int i = 0; i < _children.Count; i++)
            {
                if (lineIndex < _children[i].LineStart)
                    continue;

                if (lineIndex >= _children[i].LineStart)
                    break;

                _index = i;
                _inParent = false;
                return true;
            }

            _inParent = true;
            return true;
        }

        public bool Step()
        {
            // This approach as a whole is annoyingly finicky

            // 9 basic cases:

            if (_children.Count == 0)
            {
                // 1: No subexpressions (i.e. a single element)
                // (3 sub-cases)

                // end already reached. This is redundant
                //if (_index == 0)
                    //return false;

                if (_inParent)
                {
                    _index = 0;
                    Current = null;
                    return false;
                }
                else
                {
                    Current = new TextAssociation(
                            _parent.StartToken.Start,
                            _parent.EndToken.End,
                            _parent);
                }
            }

            else if (_index == -1 && _inParent == false)
            {
                // 2: The enumeration has yet to start

                int firstStart = _parent.StartToken.Start;
                int nextStart = _children[0].CharacterStart;

                if (firstStart == nextStart)
                    StartNextSubexpression();

                else
                {
                    _inParent = true;
                    Current = new TextAssociation(firstStart, nextStart, _parent);
                }
            }

            else if (_index == -1)
            {
                // 3: current position is in the parent, before the first subExpression

                StartNextSubexpression();
            }

            else if (_index == _children.Count)
            {
                // 4: Current position is after the last subExpression.

                // This works regardless of where _inParent is true or not - either
                // the end has been reached before, or has just been reached now
                Current = null;
                return false;
            }

            else if (_index == _children.Count - 1 && _children[_index].Step())
            {
                // 5: Current position is at the last subExpression, and stays there

                Current = _children[_index].Current;
            }

            else if (_index == _children.Count - 1)
            {
                // 6: Current position is at the last subExpression, and leaves it

                Current = null;

                int previousEnd = _children[_index].CharacterEnd;
                int nextEnd = _parent.EndToken.End;

                if (previousEnd < nextEnd)
                {
                    _inParent = true;
                    Current = new TextAssociation(previousEnd, nextEnd, _parent);
                }

                _index = _children.Count;
            }

            else if (_inParent)
            {
                // 7: Current position is between subExpressions

                StartNextSubexpression();
            }

            else if (_children[_index].Step())
            {
                // 8: Current position is within a subExpression, and
                // stays there

                Current = _children[_index].Current;
            }

            else
            {
                // 9: Current position is within a subExpression, and leaves it

                int currentEnd = _children[_index].CharacterEnd;
                int nextStart = _children[_index].CharacterStart;

                if (currentEnd == nextStart)
                {
                    StartNextSubexpression();
                }
                else
                {
                    _inParent = true;
                    Current = new TextAssociation(currentEnd, nextStart, _parent);
                }
            }

            return true;
        }

        private void StartNextSubexpression()
        {
            _index++;

            _children[_index].Reset();
            _children[_index].Step();

            Current = _children[_index].Current;
        }
    }
}
