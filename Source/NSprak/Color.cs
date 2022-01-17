using System;

namespace NSprak
{
    public struct Color
    {
        public static readonly Color
            White = new Color(255, 255, 255),
            Red = new Color(255, 0, 0),
            Green = new Color(0, 255, 0);

        public byte R, G, B;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(double r, double g, double b)
        {
            r = Math.Clamp(r, 0, 1);
            g = Math.Clamp(g, 0, 1);
            b = Math.Clamp(b, 0, 1);

            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
        }
    }
}
