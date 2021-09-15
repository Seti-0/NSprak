using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace NSprakIDE.Controls.Screen.Layers
{
    public class TestLayer : ScreenLayer
    {
        public override void Render(DrawingContext context, Rect targetRect)
        {
            Brush[,] brushes = new Brush[2, 2];
            brushes[0, 0] = new SolidColorBrush(Colors.Red);
            brushes[0, 1] = new SolidColorBrush(Colors.Blue);
            brushes[1, 0] = new SolidColorBrush(Colors.Green);
            brushes[1, 1] = new SolidColorBrush(Colors.Orange);

            string[,] characters = new string[2, 2];
            characters[0, 0] = "0";
            characters[0, 1] = "1";
            characters[1, 0] = "2";
            characters[1, 1] = "3";

            double w = Screen.CharWidth;
            double h = Screen.CharHeight;
            string text = "";

            for (int j = 0; j < Screen.HeightChars; j++)
            {
                for (int i = 0; i < Screen.WidthChars; i++)
                {
                    Brush brush = brushes[i % 2, j % 2];
                    Rect rect = new Rect(i * w,
                        j * h,
                        w, h
                    );
                    context.DrawRectangle(brush, null, rect);

                    text += characters[i % 2, j % 2].ToString();
                }
                text += "\n";
            }

            Brush textBrush = new SolidColorBrush(Colors.White);
            FormattedText formatted = Screen.GetFormattedText(text, textBrush);

            context.DrawText(formatted, new Point(0, 0));
        }
    }
}
