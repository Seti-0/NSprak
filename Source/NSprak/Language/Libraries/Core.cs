using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Language.Libraries
{
    public static class Core
    {
        private static Random _random = new Random();

        public static SprakUnit Print(ExecutionContext context, SprakString input)
        {
            context.Computer.StandardOut?.Print(input.Value);
            return SprakUnit.Value;
        }

        public static SprakNumber Count(SprakArray array)
        {
            return new SprakNumber(array.Value.Count);
        }

        public static SprakUnit Append(SprakArray array, Value newElement)
        {
            array.Value.Add(newElement);
            return SprakUnit.Value;
        }

        public static SprakNumber Pow(SprakNumber a, SprakNumber b)
        {
            return new SprakNumber(Math.Pow(a.Value, b.Value));
        }

        public static SprakNumber Sqrt(SprakNumber a)
        {
            return new SprakNumber(Math.Sqrt(a.Value));
        }

        public static SprakNumber Round(SprakNumber a)
        {
            return new SprakNumber(Math.Round(a.Value));
        }

        public static SprakNumber Random()
        {
            return new SprakNumber(_random.NextDouble());
        }
    }
}
