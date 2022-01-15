using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Language;

namespace NSprak.Tests.Types
{
    public class Equality : TestCommand
    {
        public static bool IsValidValue(string text)
        {
            if (Value.TryParse(text, out _))
                return true;

            if (Symbols.IsValidWord(text))
                return true;

            return false;
        }

        public string Left { get; }

        public string Right { get; }

        public Equality(string left, string right)
        {
            Left = left;
            Right = right;
        }
    }
}
