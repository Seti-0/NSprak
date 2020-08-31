using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Language
{
    public class Keywords
    {
        public const string
            If = "if",
            Else = "else",
            Loop = "loop",
            In = "in",
            From = "from",
            To = "to",
            Break = "break",
            Continue = "continue",
            Return = "return",
            End = "end";

        public static readonly IReadOnlyList<string> All;

        static Keywords()
        {
            All = new string[]
            {
                If, Loop, From, To, In, Break, Continue, Return, End
            };
        }

        public static bool IsKeyword(string text)
        {
            return All.Contains(text);
        }
    }
}
