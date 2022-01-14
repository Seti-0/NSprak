using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using NSprak;

using NSprakIDE.Controls.Screen.Layers;

namespace NSprakIDE.Controls.Screen
{
    using WindowsColor = System.Windows.Media.Color;
    using SprakColor = Color;

    public class ComputerScreen : IComputerScreen
    {
        private readonly FixedSizeScreen _view;

        private readonly TextLayer _text = new TextLayer();
        private readonly GraphicalLayer _graphics = new GraphicalLayer();
        private readonly InputLayer _input = new InputLayer();
        private readonly Dispatcher _dispatcher;

        private bool _previousHasContent = false;

        public IEnumerable<ScreenLayer> Layers { get; }

        public double Width => _view.WidthPixels;

        public double Height => _view.HeightPixels;

        public bool HasContent => _text.HasContent || _graphics.HasContent;

        public event EventHandler<EventArgs> HasContentChanged;

        public ComputerScreen(Dispatcher dispatcher, FixedSizeScreen view)
        {
            _dispatcher = dispatcher;
            _view = view;

            Layers = new ScreenLayer[]
            {
                _graphics, _text, _input
            };

            _previousHasContent = HasContent;
            void OnInvalidate(object sender, EventArgs e)
            {
                if (_previousHasContent != HasContent)
                {
                    _previousHasContent = HasContent;
                    OnHasContentChanged(EventArgs.Empty);
                }   
            }

            _text.Invalidated += OnInvalidate;
            _graphics.Invalidated += OnInvalidate;
        }

        private void Invoke(Action action)
        {
            _dispatcher.Invoke(action, DispatcherPriority.ApplicationIdle);
        }

        public void Clear()
        {
            void Action()
            {
                _text.ClearText();
                _graphics.ClearGraphics();
            }

            Invoke(Action);
        }

        public void ClearText()
        {
            Invoke(_text.ClearText);
        }

        public void DisplayGraphics()
        {
            Invoke(_graphics.DisplayGraphics);
        }

        public SprakColor GetColor()
        {
           WindowsColor color = _graphics.Color;
            return new SprakColor(color.R, color.G, color.B);
        }

        public void CancelInput()
        {
            _text.CancelInput();
        }

        public void CopyToClipboard(string content)
        {
            Application.Current.Dispatcher
                .Invoke(() => Clipboard.SetText(content));
        }

        public string Input(string promt)
        {
            return _text.Input(promt, _dispatcher);
        }

        public bool IsKeyPressed(string key)
        {
            // In contrast to most other screen layer operations, this one
            // is thread-safe.
            return _input.IsKeyPressed(key);
        }

        public void Line(double x1, double y1, double x2, double y2)
        {
            Invoke(() => _graphics.AddLine(x1, y1, x2, y2));
        }

        public void SetPrintColor(SprakColor color)
        {
            WindowsColor converted = WindowsColor
                .FromRgb(color.R, color.G, color.B);

            Invoke(() => _text.Color = converted);
        }

        public void Print(string line)
        {
            Invoke(() => _text.Print(line));
        }

        public void PrintS(string text)
        {
            Invoke(() => _text.PrintS(text));
        }

        public void Rect(double x, double y, double w, double h)
        {
            Invoke(() => _graphics.AddRect(x, y, w, h));
        }

        public void SetColor(SprakColor color)
        {
            WindowsColor converted = WindowsColor
                .FromRgb(color.R, color.G, color.B);

            Invoke(() => _graphics.Color = converted);
        }

        public void Text(string text, double x, double y)
        {
            Invoke(() => _graphics.AddText(text, x, y));
        }

        protected virtual void OnHasContentChanged(EventArgs e)
        {
            HasContentChanged?.Invoke(this, e);
        }
    }
}
