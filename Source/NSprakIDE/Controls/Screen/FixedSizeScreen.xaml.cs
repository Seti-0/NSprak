using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Screen.Layers;
using NSprakIDE.Themes;

namespace NSprakIDE.Controls.Screen
{
    public partial class FixedSizeScreen : UserControl
    {
        private readonly List<ScreenLayer> _layers = new List<ScreenLayer>();

        private readonly FormattedText _em;
        private double _fontSize;
        private readonly DpiScale _dpi = new DpiScale(1, 1);
        private readonly Typeface _face = new Typeface("Consolas");

        public int WidthChars { get; set; } = 65;

        public int HeightChars { get; set; } = 30;

        public double TerminalFontSize => _fontSize;

        public double CharWidth => _em.Width;

        public double CharHeight => _em.Height;

        public double TerminalWidth => _em.Width * WidthChars;

        public double TerminalHeight => _em.Height * HeightChars;

        public FixedSizeScreen()
        {
            InitializeComponent();

            _fontSize = 5;
            _em = GetFormattedText("M", Foreground);

            SizeChanged += FixedSizeScreen_SizeChanged;
        }

        private void FixedSizeScreen_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public void ClearLayers()
        {
            _layers.Clear();
        }

        public void SetLayers(IEnumerable<ScreenLayer> layers)
        {
            _layers.Clear();
            _layers.AddRange(layers);
            foreach (ScreenLayer layer in layers)
                layer.Screen = this;

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            ApplySuitableFontSize();

            if (RenderSize.Width * RenderSize.Height == 0)
                return;

            Brush border = Theme.GetBrush(Theme.Screen.Border);
            drawingContext.DrawRectangle(border, null, new Rect(RenderSize));

            double x = (RenderSize.Width - TerminalWidth) / 2;
            double y = (RenderSize.Height - TerminalHeight) / 2;
            double w = TerminalWidth;
            double h = TerminalHeight;

            Brush background = Theme.GetBrush(Theme.Screen.Background);
            Rect targetRect = new Rect(x, y, w, h);
            drawingContext.DrawRectangle(background, null, targetRect);

            RectangleGeometry clip = new RectangleGeometry(targetRect);
            drawingContext.PushClip(clip);

            foreach (ScreenLayer layer in _layers)
                layer.Render(drawingContext, targetRect);

            drawingContext.Pop();
        }

        private void ApplySuitableFontSize()
        {
            if (RenderSize.Width == 0 || RenderSize.Height == 0)
            {
                _fontSize = 0;
                return;
            }

            // I don't know exactly how font size works in WPF, but I'd hazard
            // a guess that there is a roughly linear relationship between 
            // font width/height

            double targetW = RenderSize.Width / WidthChars;
            double targetH = RenderSize.Height / HeightChars;

            double x1 = 10;
            _em.SetFontSize(x1);
            double w1 = _em.Width;
            double h1 = _em.Height;

            double x2 = 20;
            _em.SetFontSize(x2);
            double w2 = _em.Width;
            double h2 = _em.Height;

            double mw = (w2 - w1) / (x2 - x1);
            double cw = w1 - mw * x1;
            double xw = (targetW - cw) / mw;

            double mh = (h2 - h1) / (x2 - x1);
            double ch = h1 - mh * x1;
            double xh = (targetH - ch) / mh;

            double fontSize = Math.Min(xw, xh);
            _fontSize = fontSize;

            if (fontSize > 0)
                _em.SetFontSize(fontSize);
        }

        public FormattedText GetFormattedText(string text, Brush brush)
        {
            return new FormattedText(text, CultureInfo.InvariantCulture, 
                FlowDirection.LeftToRight, _face, _fontSize, 
                brush, _dpi.PixelsPerDip);
        }
    }
}
