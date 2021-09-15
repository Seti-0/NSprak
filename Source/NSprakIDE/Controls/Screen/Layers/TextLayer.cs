using NSprakIDE.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NSprakIDE.Controls.Screen.Layers
{
    public class TextLayer : ScreenLayer
    {
        private List<FormattedText> _lines = new List<FormattedText>();
        private string _lastLine = "";

        public void Print(string text)
        {
            _lastLine += text;

            Brush brush = Theme.GetBrush(Theme.Screen.Text);
            _lines.Add(Screen.GetFormattedText(_lastLine, brush));
            _lastLine = "";

            Invalidate();
        }

        public void PrintS(string text)
        {
            _lastLine += text;

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

            int startIndex = _lines.Count - Screen.HeightChars;
            if (_lastLine.Length > 0)
                startIndex++;

            if (startIndex < 0)
                startIndex = 0;

            Point cursor = new Point(targetRect.X + 10, targetRect.Y + 10);
            for (int i = startIndex; i < _lines.Count; i++)
            {
                _lines[i].SetFontSize(Screen.FontSize);
                context.DrawText(_lines[i], cursor);
                cursor.Y += _lines[i].Height;
            }
            
            if (_lastLine.Length > 0)
            {
                Brush brush = Theme.GetBrush(Theme.Screen.Text);
                FormattedText temp = Screen.GetFormattedText(_lastLine, brush);
                context.DrawText(temp, cursor);
            }
        }
    }
}
