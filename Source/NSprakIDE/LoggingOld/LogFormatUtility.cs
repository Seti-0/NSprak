using System;
using System.Text.RegularExpressions;

namespace NSprakIDE.Logging
{
    public static class LogFormatUtility
    {
        public static void ApplyIndent(IWriter writer, int indent)
        {
            if (indent < 0)
                indent = 0;

            writer.Write(new string('\t', indent));
        }

        public static void WritePrefix(IWriter writer, LogEntry entry, ref string _lastDate)
        {
            string dateText = entry.Time.ToShortDateString();

            if (dateText != _lastDate)
            {
                writer.WriteLine($"[{dateText}]");
                _lastDate = dateText;
            }


            string type = Enum.GetName(typeof(LogType), entry.Type);
            writer.Write($"[{entry.Time.ToShortTimeString()}][{type}] ");
        }

        public static bool Split(string input, string pattern, out string upto, out string after)
        {
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                upto = input.Substring(0, match.Index + 1);
                after = input.Substring(match.Index + 1);
                return true;
            }
            else
            {
                upto = null;
                after = null;
                return false;
            }
        }

        public static void SplitMethodSignature(string input, out string name, out string arguments)
        {
            int argumentIndex = input.IndexOf('(');
            int genericIndex = input.IndexOf('<');

            int index = input.Length - 1;

            if (argumentIndex > -1)
                index = argumentIndex;

            if (genericIndex > -1 && genericIndex < argumentIndex)
                index = genericIndex;

            name = input.Substring(0, index);
            arguments = input.Substring(index);
        }
    }
}
