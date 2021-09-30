using NSprakIDE.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NSprakIDE.Controls.Screen.Layers
{
    public class TextLayer : ScreenLayer
    {
        private readonly List<FormattedText> _lines = new List<FormattedText>();
        private string _lastLine = "";

        private readonly AutoResetEvent _inputIdleEvent
            = new AutoResetEvent(true);
        private readonly AutoResetEvent _inputReadyEvent 
            = new AutoResetEvent(false);

        private bool _input = false;
        private string _inputText = "";

        private bool _cursorVisible = true;
        private readonly DispatcherTimer _blinkTimer = new DispatcherTimer();

        public bool HasContent => _lines.Count > 0
            || _lastLine.Length > 0 || _inputText.Length > 0;

        public TextLayer()
        {
            _blinkTimer.Interval = TimeSpan.FromSeconds(0.5);

            _blinkTimer.Tick += (obj, e) =>
            {
                _cursorVisible = !_cursorVisible;
                Invalidate();
            };
        }

        public void CancelInput()
        {
            if (_input)
                EndInput();
        }

        public string Input(string promt, Dispatcher dispatcher)
        {
            _inputIdleEvent.WaitOne();

            dispatcher.Invoke(() => StartInput(promt));

            _inputReadyEvent.WaitOne();

            return CollectInput();
        }

        public void Print(string text)
        {
            PrintS(text + "\n");
        }

        public void PrintS(string text)
        {
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length - 1; i++)
                PrintSingleLine(lines[i]);

            _lastLine += lines[^1];

            Invalidate();
        }

        private void PrintSingleLine(string text)
        {
            _lastLine += text;

            Brush brush = Theme.GetBrush(Theme.Screen.Text);
            _lines.Add(Screen.GetFormattedText(_lastLine, brush));
            _lastLine = "";

            Invalidate();
        }

        public void ClearText()
        {
            _lines.Clear();
            _lastLine = "";

            Invalidate();
        }

        public override void Render(DrawingContext context, Rect targetRect)
        {
            if (Screen.HeightChars == 0)
                return;

            // Leaving a border of 1 char height above and below, hence the 
            // minus 2.
            int effectiveTerminalHeight = Screen.HeightChars - 2;

            int startIndex = _lines.Count - effectiveTerminalHeight;
            if (_lastLine.Length > 0)
                startIndex++;

            if (startIndex < 0)
                startIndex = 0;

            double border = Screen.CharHeight;
            Point cursor = new Point(targetRect.X + border, targetRect.Y + border);
            for (int i = startIndex; i < _lines.Count; i++)
            {
                _lines[i].SetFontSize(Screen.TerminalFontSize);
                context.DrawText(_lines[i], cursor);
                cursor.Y += _lines[i].Height;
            }

            Brush brush = Theme.GetBrush(Theme.Screen.Text);

            string text = _lastLine;
            if (_input)
                text += _inputText;

            if (text.Length > 0)
            {
                FormattedText temp = Screen.GetFormattedText(text, brush);
                context.DrawText(temp, cursor);
                cursor.X += temp.WidthIncludingTrailingWhitespace;
            }

            if (_input && _cursorVisible && Screen.IsFocused)
            {
                context.DrawRectangle(brush, null, new Rect(cursor,
                    new Size(Screen.CharWidth, Screen.CharHeight)));
            }
        }

        public override void OnTextInput(TextCompositionEventArgs e)
        {
            if (e.Text.Length == 0 || !_input)
                return;

            if (e.Text[0] == '\r')
                EndInput();

            else if (e.Text[0] == '\b' & _inputText.Length > 0)
            {
                _inputText = _inputText[0..^1];
                Invalidate();
            }
            else
            {
                _inputText += e.Text;
                Invalidate();
            }
        }

        private void StartInput(string promt)
        {
            _inputIdleEvent.Reset();

            _input = true;
            _inputText = "";

            PrintS(promt);

            _cursorVisible = true;
            Invalidate();
            _blinkTimer.Start();

            Screen.Focus();
        }

        private void EndInput()
        {
            _input = false;
            Print(_inputText);

            _inputReadyEvent.Set();
            _blinkTimer.Stop();
        }

        private string CollectInput()
        {
            string result = _inputText;
            _inputText = "";

            _inputReadyEvent.Reset();
            _inputIdleEvent.Set();

            return result;
        }
    }
}
