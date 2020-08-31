using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NSprakIDE
{
    public class TextBoxConsole
    { 
        private static Color GetColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: return Color.FromRgb(0, 0, 0);
                case ConsoleColor.Blue: return Color.FromRgb(22, 128, 255);
                case ConsoleColor.Cyan: return Color.FromRgb(79, 244, 244);
                case ConsoleColor.DarkBlue: return Color.FromRgb(29, 34, 211);
                case ConsoleColor.DarkCyan: return Color.FromRgb(71, 179, 171);
                case ConsoleColor.DarkGray: return Color.FromRgb(100, 100, 100);
                case ConsoleColor.DarkGreen: return Color.FromRgb(0, 100, 0);
                case ConsoleColor.DarkMagenta: return Color.FromRgb(138, 0, 138);
                case ConsoleColor.DarkRed: return Color.FromRgb(138, 0, 0);
                case ConsoleColor.DarkYellow: return Color.FromRgb(189, 164, 12);
                case ConsoleColor.Gray: return Color.FromRgb(128, 128, 128);
                case ConsoleColor.Green: return Color.FromRgb(51, 204, 51);
                case ConsoleColor.Magenta: return Color.FromRgb(255, 0, 255);
                case ConsoleColor.Red: return Color.FromRgb(255, 0, 0);
                case ConsoleColor.White: return Color.FromRgb(255, 255, 255);
                case ConsoleColor.Yellow: return Color.FromRgb(255, 230, 77);

                default: return Color.FromRgb(255, 0, 255);
            }
        }

        public string ID { get; }

        public ConsoleColor ForegroundColor { get; set; }

        public RichTextBox Target { get; set; }

        public TextBoxConsole(string id, RichTextBox target)
        {
            ID = id;
            Target = target;
        }

        public void Write(string text)
        {
            if (Target == null)
                return;

            Target.Dispatcher.Invoke(() => WriteUnsafe(text));
        }

        private void WriteUnsafe(string text)
        {
            /* This method must be called from the GUI thread */

            var range = new TextRange(Target.Document.ContentEnd, Target.Document.ContentEnd);
            range.Text = text ?? "";

            var color = GetColor(ForegroundColor);
            var brush = new SolidColorBrush(color);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);

            Target.ScrollToEnd();
        }

        public void WriteLine(string text = "")
        {
            if (Target == null)
                return;

            text = text ?? "";

            Write(text + "\n");
        }
    }
}
