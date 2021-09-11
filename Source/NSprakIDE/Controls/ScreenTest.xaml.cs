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

namespace NSprakIDE.Controls
{
    /// <summary>
    /// Interaction logic for Screen.xaml
    /// </summary>
    public partial class Screen : UserControl
    {
        public Screen()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Brush[,] brushes = new Brush[2, 2];
            brushes[0, 0] = new SolidColorBrush(Colors.Red);
            brushes[0, 1] = new SolidColorBrush(Colors.Blue);
            brushes[1, 0] = new SolidColorBrush(Colors.Green);
            brushes[1, 1] = new SolidColorBrush(Colors.Orange);

            double fontPtSize = 50;
            FormattedText EM = new FormattedText(
                    "M",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    fontPtSize,
                    new SolidColorBrush(Colors.White),
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                    );

            double w = EM.Width;
            double h = EM.Height;

            int N = (int)Math.Floor(RenderSize.Width / w);
            int M = (int)Math.Floor(RenderSize.Height / h);

            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                {
                    Brush current = brushes[i % 2, j % 2];
                    Rect rect = new Rect(i*w, j*h, w, h);
                    drawingContext.DrawRectangle(current, null, rect);
                }

            FormattedText helloworld = new FormattedText(
                    "Hello World abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN OPQRSTUVW XYZ1234567890!\"£$%^&*()",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    fontPtSize,
                    new SolidColorBrush(Colors.White),
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                    );

            helloworld.MaxTextWidth = RenderSize.Width;

            drawingContext.DrawText(helloworld, new Point(0, 0));
        }
    }
}
