using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

using NSprakIDE.Logging;

namespace NSprakIDE.Controls.Output
{
    public static class ColorHelper
    {
        public static Color GetColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => Color.FromRgb(0, 0, 0),
                ConsoleColor.Blue => Color.FromRgb(22, 128, 255),
                ConsoleColor.Cyan => Color.FromRgb(79, 244, 244),
                ConsoleColor.DarkBlue => Color.FromRgb(29, 34, 211),
                ConsoleColor.DarkCyan => Color.FromRgb(71, 179, 171),
                ConsoleColor.DarkGray => Color.FromRgb(100, 100, 100),
                ConsoleColor.DarkGreen => Color.FromRgb(0, 100, 0),
                ConsoleColor.DarkMagenta => Color.FromRgb(138, 0, 138),
                ConsoleColor.DarkRed => Color.FromRgb(138, 0, 0),
                ConsoleColor.DarkYellow => Color.FromRgb(189, 164, 12),
                ConsoleColor.Gray => Color.FromRgb(128, 128, 128),
                ConsoleColor.Green => Color.FromRgb(51, 204, 51),
                ConsoleColor.Magenta => Color.FromRgb(255, 0, 255),
                ConsoleColor.Red => Color.FromRgb(255, 0, 0),
                ConsoleColor.White => Color.FromRgb(255, 255, 255),
                ConsoleColor.Yellow => Color.FromRgb(255, 230, 77),
                _ => Color.FromRgb(255, 0, 255),
            };
        }
    }

    public class OutputLogWriter : IWriter
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

        public object Mark()
        {
            return Log.Mark();
        }

        public bool ClearMark(object id)
        {
            return Log.ClearMark(id);
        }

        public bool Edit(object id, string newText)
        {
            return Log.Edit(id, newText);
        }
    }
}
