﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak
{
    public interface IComputerScreen
    {
        public double Height { get; }

        public double Width { get; }

        public void SetColor(Color color);

        public Color GetColor();

        // Text Layer

        public void Print(string line);

        public void PrintS(string text);

        public void ClearText();

        public string Input(string promt);

        public void CancelInput();

        // Graphical Layer

        public void Line(double x1, double y1, double x2, double y2);

        public void Rect(double x, double y, double w, double h);

        public void Text(string text, double x, double y);

        public void DisplayGraphics();

        // Clipboard

        public void CopyToClipboard(string content);
    }
}
