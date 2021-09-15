using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using NSprak;

using NSprakIDE.Controls.Screen.Layers;

namespace NSprakIDE.Controls.Screen
{
    using WindowsColor = System.Windows.Media.Color;
    using SprakColor = Color;

    public class ComputerScreen : IComputerScreen
    {
        private readonly TextLayer _text = new TextLayer();
        private readonly GraphicalLayer _graphics = new GraphicalLayer();
        private Dispatcher _dispatcher;

        public IEnumerable<ScreenLayer> Layers { get; }

        public ComputerScreen(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            Layers = new ScreenLayer[]
            {
                _text, _graphics
            };
        }

        private void Invoke(ScreenLayer layer, Action action)
        {
            _dispatcher.Invoke(action);
        }

        public void ClearText()
        {
            Invoke(_text, _text.ClearText);
        }

        public void DisplayGraphics()
        {
            Invoke(_graphics, _graphics.DisplayGraphics);
        }

        public SprakColor GetColor()
        {
           WindowsColor color = _graphics.Color;
            return new SprakColor(color.R, color.G, color.B);
        }

        public void Line(double x1, double y1, double x2, double y2)
        {
            Invoke(_graphics, () => _graphics.AddLine(x1, y1, x2, y2));
        }

        public void Print(string line)
        {
            Invoke(_text, () => _text.Print(line));
        }

        public void PrintS(string text)
        {
            Invoke(_text, () => _text.PrintS(text));
        }

        public void Rect(double x, double y, double w, double h)
        {
            Invoke(_graphics, () => _graphics.AddRect(x, y, w, h));
        }

        public void SetColor(SprakColor color)
        {
            WindowsColor converted = WindowsColor
                .FromRgb(color.R, color.G, color.B);

            Invoke(_graphics, () => _graphics.Color = converted);
        }

        public void Text(string text, double x, double y)
        {
            Invoke(_graphics, () => _graphics.AddText(text, x, y));
        }
    }
}
