using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NSprakIDE
{
    public static class StringHelper
    {
        public static string Simplify(string input)
        {
            return Regex.Replace(input, @"[^a-zA-Z0-9]", ToHex);
        }

        private static string ToHex(Match match)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(match.Value);
            IEnumerable<string> hexes = bytes.Select(x => string.Format("{0:X}", x));
            return string.Join('_', hexes);
        }
    }
}
