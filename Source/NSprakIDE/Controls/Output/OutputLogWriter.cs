using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

using Serilog.Core;
using Serilog.Events;

using NSprakIDE.Logging;

namespace NSprakIDE.Controls.Output
{
    public static class ColorHelper
    {
        public static Color GetColor(ConsoleColor color)
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
    }

    public class OutputLogWriter : IColoredWriter
    {
        private ConsoleColor _color;
        
        public OutputLog Log { get; }

        public ConsoleColor Color
        {
            get => _color;

            set
            {
                _color = value;
                Log.Color = ColorHelper.GetColor(_color);
            }
        }


        public OutputLogWriter(OutputLog output)
        {
            Log = output;
        }

        public void Begin()
        {
            Color = ConsoleColor.DarkGray;
            Log.WriteLine($"Log {Log.Name} beginning at {DateTime.Now}");
        }

        public void End()
        {
            Color = ConsoleColor.DarkGray;
            Log.WriteLine($"Log {Log.Name} ending at {DateTime.Now}");
        }

        public void Write(string text)
        {
            Log.Write(text);
        }

        public void WriteLine(string text = "")
        {
            Log.WriteLine(text);
        }
    }
}
