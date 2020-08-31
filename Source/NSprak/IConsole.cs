using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NSprak
{
    public interface IConsole
    {
        public void SetColor(Color color);

        public Color GetColor();

        public void Print(string line);

        public void PrintS(string text);
    }
}
