namespace NSprak
{
    public struct Color
    {
        public static readonly Color
            White = new Color(255, 255, 255),
            Red = new Color(255, 0, 0);

        public byte R, G, B;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
