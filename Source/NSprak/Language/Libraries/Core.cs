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

        public static SprakNumber Cos(SprakNumber x)
        {
            return new SprakNumber(Math.Cos(x.Value));
        }

        public static SprakNumber Count(SprakArray array)
        {
            return new SprakNumber(array.Value.Count);
        }

        // Need to figure out the clipboard at some point.



        // H

        public static SprakBoolean HasFunction(ExecutionContext context,
            SprakString name)
        {
            bool exists = context.Computer.Resolver
                .FindFunctions(name.Value)
                .Any();

            return new SprakBoolean(exists);
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

        public static SprakNumber Round(SprakNumber a)
        {
            return new SprakNumber(Math.Round(a.Value));
        }

        // S

        public static SprakNumber Sqrt(SprakNumber a)
        {
            return new SprakNumber(Math.Sqrt(a.Value));
        }
    }
}
