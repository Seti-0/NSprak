using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using NSprak;
using NSprakIDE.Controls.Output;

namespace NSprakIDE.Controls.General
{
    public class ComputerOutput : IConsole
    {
        private OutputLog _output;

        public ComputerOutput(OutputLog output)
        {
            _output = output;
            SetColor(Color.White);
        }

        public Color GetColor()
        {
            return Color.FromArgb(
                _output.Color.A,
                _output.Color.R,
                _output.Color.G,
                _output.Color.B
                );
        }

        public void Print(string line)
        {
            _output.WriteLine(line);
        }

        public void PrintS(string text)
        {
            _output.Write(text);
        }

        public void SetColor(Color color)
        {
            _output.Color = System.Windows.Media.Color.FromArgb(
                color.A,
                color.R,
                color.G,
                color.B
                );
        }
    }
}
