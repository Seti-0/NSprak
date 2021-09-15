using NSprakIDE.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NSprakIDE.Controls.Screen.Layers
{
    public class GraphicalLayer : ScreenLayer
    {
        private class Entry {}

        private class LineEntry : Entry
        {
            public Pen Pen;
            public Point Start, End;
            public Point ScreenStart, ScreenEnd;
        }

        private class RectEntry : Entry
        {
            public Brush Brush;
            public Rect Content;
            public Rect ScreenRect;
        }

        private class TextEntry : Entry
        {
            public Point Origin;
            public FormattedText Content;
            public Point ScreenOrigin;
        }

        private List<Entry> _nextEntries = new List<Entry>();

        private List<Entry> _currentEntries = new List<Entry>();

        private Color _color;
        private Brush _brush;
        private Pen _pen;

        public Color Color
        {
            get => _color;

            set
            {
                _color = value;
                _brush = new SolidColorBrush(value);
                _pen = new Pen(_brush, 1);
            }
        }

        public void AddLine(double x1, double y1, double x2, double y2)
        {
            LineEntry entry = new LineEntry
            {
                Pen = _pen,
                Start = new Point(x1, y1),
                End = new Point(x2, y2)
            };

            _nextEntries.Add(entry);
        }

        public void AddRect(double x, double y, double w, double h)
        {
            RectEntry entry = new RectEntry
            {
                Brush = _brush,
                Content = new Rect(x, y, w, h)
            };

            _nextEntries.Add(entry);
        }

        public void AddText(string text, double x, double y)
        {
            Brush brush = _brush;
            FormattedText formatted = Screen.GetFormattedText(text, brush);

            TextEntry entry = new TextEntry
            {
                Origin = new Point(x, y),
                Content = formatted
            };

            _nextEntries.Add(entry);
        }
        public void DisplayGraphics()
        {
            _currentEntries.Clear();

            List<Entry> current = _currentEntries;
            _currentEntries = _nextEntries;
            _nextEntries = current;
            Invalidate();
        }

        public override void Render(DrawingContext context, Rect targetRect)
        {
            foreach (Entry entry in _currentEntries)
            {
                switch (entry)
                {
                    case LineEntry line:
                        line.ScreenStart.X = targetRect.X + line.Start.X;
                        line.ScreenStart.Y = targetRect.Y + line.Start.Y;
                        line.ScreenEnd.X = targetRect.X + line.End.X;
                        line.ScreenEnd.Y = targetRect.Y + line.End.Y;
                        context.DrawLine(line.Pen, line.ScreenStart, line.ScreenEnd);
                        break;

                    case RectEntry rect:
                        rect.ScreenRect.X = targetRect.X + rect.Content.X;
                        rect.ScreenRect.Y = targetRect.Y + rect.Content.Y;
                        context.DrawRectangle(rect.Brush, null, rect.ScreenRect);
                        break;

                    case TextEntry text:
                        text.ScreenOrigin.X = targetRect.X + text.Origin.X;
                        text.ScreenOrigin.Y = targetRect.Y + text.Origin.Y;
                        text.Content.SetFontSize(Screen.FontSize);
                        context.DrawText(text.Content, text.ScreenOrigin);
                        break;
                }
            }
        }
    }
}
