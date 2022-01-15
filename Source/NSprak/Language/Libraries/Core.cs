using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Language.Libraries
{
    public static class Core
    {
        // Using https://steamcommunity.com/sharedfiles/filedetails/?id=612257262
        // as reference.

        private readonly static Random _random = new Random();

        // A

        public static SprakUnit Append(SprakArray array, Value newElement)
        {
            array.Value.Add(newElement);
            return SprakUnit.Value;
        }

        // B

        // What does the broadcast function do?

        // C

        public static SprakNumber CharToInt(SprakString text)
        {
            if (double.TryParse(text.Value, out double result))
                return new SprakNumber(result);
            else
                return new SprakNumber(double.NaN);
        }

        public static SprakUnit ClearText(ExecutionContext context)
        {
            context.Computer.Screen?.ClearText();
            return SprakUnit.Value;
        }

        public static SprakUnit Color(
            ExecutionContext context, SprakNumber r, SprakNumber g, SprakNumber b)
        {
            context.Computer?.Screen.SetColor(new Color(
                r.Value,
                g.Value,
                b.Value
            ));

            return SprakUnit.Value;
        }

        public static SprakUnit CopyToClipboard(
            ExecutionContext context, SprakString content)
        {
            context.Computer.Screen?.CopyToClipboard(content.Value);
            return SprakUnit.Value;
        }

        public static SprakNumber Cos(SprakNumber radians)
        {
            return new SprakNumber(Math.Cos(radians.Value));
        }

        public static SprakNumber Count(SprakArray array)
        {
            return new SprakNumber(array.Value.Count);
        }

        // Need to figure out the clipboard at some point.

        // D

        public static SprakUnit DisplayGraphics(ExecutionContext context)
        {
            context.Computer.Screen?.DisplayGraphics();
            return SprakUnit.Value;
        }

        // H

        public static SprakBoolean HasFunction(ExecutionContext context,
            SprakString name)
        {
            bool exists = context.SignatureResolver
                .FindFunctions(name.Value)
                .Any();

            return SprakBoolean.From(exists);
        }

        public static SprakNumber Height(ExecutionContext context)
        {
            return new SprakNumber(context.Computer.Screen?.Height ?? 0);
        }

        public static SprakArray HSVToRGB(
            SprakNumber h, SprakNumber s, SprakNumber v)
        {
            // All sprak color values are in [0, 1]
            double H = h.Value * 360;
            double S = s.Value;
            double V = v.Value;

            double C = V * S;
            double Hn = H / 60;
            double X = C * (1 - Math.Abs((Hn % 2) - 1));

            double R, G, B;

            if (Hn < 1) 
            {
                R = C; 
                G = X; 
                B = 0;
            }
            else if (Hn < 2)
            {
                R = X;
                G = C;
                B = 0;
            }
            else if (Hn < 3)
            {
                R = 0;
                G = C;
                B = X;
            }
            else if (Hn < 4)
            {
                R = 0;
                G = X;
                B = C;
            }
            else if (Hn < 5)
            {
                R = X;
                G = 0;
                B = C;
            }
            else if (Hn < 6)
            {
                R = C;
                G = 0;
                B = X;
            }
            else
            {
                // I'm not sure if this else case
                // is possible.
                R = 0;
                G = 0;
                B = 0;
            }

            double m = V - C;
            R += m; G += m; B += m;

            R = Math.Clamp(R, 0, 1);
            G = Math.Clamp(G, 0, 1);
            B = Math.Clamp(B, 0, 1);

            SprakNumber r = new SprakNumber(R);
            SprakNumber g = new SprakNumber(G);
            SprakNumber b = new SprakNumber(B);
            return new SprakArray(new List<Value>() { r, g, b });
        }

        // I

        public static SprakString Input(
            ExecutionContext context, SprakString promt)
        {
            string result = context.Computer.Screen?.Input(promt.Value);
            return new SprakString(result);
        }

        public static SprakBoolean IsKeyPressed(
            ExecutionContext context, SprakString key)
        {
            bool result = context.Computer?.Screen.IsKeyPressed(key.Value) ?? false;
            return SprakBoolean.From(result);
        }

        // L

        public static SprakUnit Line(ExecutionContext context,
            SprakNumber x1, SprakNumber y1, SprakNumber x2, SprakNumber y2)
        {
            context.Computer.Screen?.Line(x1.Value, y1.Value, x2.Value, y2.Value);
            return SprakUnit.Value;
        }

        // M

        public static SprakNumber Max(SprakNumber a, SprakNumber b)
        {
            return new SprakNumber(Math.Max(a.Value, b.Value));
        }

        public static SprakNumber Min(SprakNumber a, SprakNumber b)
        {
            return new SprakNumber(Math.Min(a.Value, b.Value));
        }

        // P

        public static SprakNumber Pow(SprakNumber a, SprakNumber b)
        {
            return new SprakNumber(Math.Pow(a.Value, b.Value));
        }

        public static SprakUnit Print(ExecutionContext context, SprakString input)
        {
            context.Computer.Screen?.Print(input.Value);
            return SprakUnit.Value;
        }

        public static SprakUnit PrintS(ExecutionContext context, SprakString input)
        {
            context.Computer.Screen?.PrintS(input.Value);
            return SprakUnit.Value;
        }

        // R

        public static SprakNumber Random()
        {
            return new SprakNumber(_random.NextDouble());
        }

        public static SprakUnit Rect(ExecutionContext context,
            SprakNumber x, SprakNumber y, SprakNumber w, SprakNumber h)
        {
            context.Computer.Screen?.Rect(x.Value, y.Value, w.Value, h.Value);
            return SprakUnit.Value;
        }

        public static SprakArray RGBToHSV(
            SprakNumber r, SprakNumber g, SprakNumber b)
        {
            // Note: all colours in sprak are normalized to [0, 1]
            double R = r.Value;
            double G = g.Value;
            double B = b.Value;

            double V = Math.Max(Math.Max(R, G), B);
            double C = V - Math.Min(Math.Min(R, G), B);

            double H;

            // Remember that V is max(R, G, B), and so should be
            // exactly R or G or B. (i.e. no need to worry about
            // floating point tolerance or an else case)

            if (C == 0)
                H = 0;
            else if (V == R)
                H = 60 * ((G - B) / C);
            else if (V == G)
                H = 60 * (2 + ((B - R) / C));
            else if (V == B)
                H = 60 * (4 + ((R - G) / C));
            else
                // This should never happen.
                H = 0;

            double S;
            if (V == 0)
                S = 0;
            else
                S = C / V;

            H = (H % 360) / 360;
            S = Math.Clamp(S, 0, 1);
            V = Math.Clamp(V, 0, 1);

            SprakNumber h = new SprakNumber(H);
            SprakNumber s = new SprakNumber(S);
            SprakNumber v = new SprakNumber(V);
            return new SprakArray(new List<Value>() { h, s, v });
        }

        public static SprakNumber Round(SprakNumber a)
        {
            return new SprakNumber(Math.Round(a.Value));
        }

        // S

        public static SprakNumber Sin(SprakNumber radians)
        {
            return new SprakNumber(Math.Sin(radians.Value));
        }

        public static SprakUnit Sleep(SprakNumber seconds)
        {
            System.Threading.Thread.Sleep((int)(seconds.Value * 1000));
            return SprakUnit.Value;
        }

        public static SprakNumber Sqrt(SprakNumber a)
        {
            return new SprakNumber(Math.Sqrt(a.Value));
        }

        // T

        public static SprakUnit Text(ExecutionContext context,
            SprakNumber x, SprakNumber y, SprakString text)
        {
            context.Computer.Screen?.Text(text.Value, x.Value, y.Value);
            return SprakUnit.Value;
        }

        public static SprakString Type(Value value)
        {
            string result;
            if (value.Type == SprakType.Connection)
                result = SprakType.Number.Text;
            else
                result = value.Type.Text;

            return new SprakString(result);
        }

        // W

        public static SprakNumber Width(ExecutionContext context)
        {
            return new SprakNumber(context.Computer.Screen?.Width ?? 0);
        }
    }
}
