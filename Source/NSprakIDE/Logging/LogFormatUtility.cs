using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace NSprakIDE.Logging
{
    public static class LogFormatUtility
    {
        public static void WritePrefix(ISimpleWriter writer, LogEvent entry, ref string _lastDate)
        {
            string dateText = entry.Timestamp.ToLocalTime().ToShortDateString();
            
            if (dateText != _lastDate)
            {
                writer.WriteLine($"[{dateText}]");
                _lastDate = dateText;
            }

            string timeText = entry.Timestamp.ToLocalTime().ToShortTimeString();

            string type = Enum.GetName(typeof(LogLevel), entry.Level);
            writer.Write($"[{timeText}][{type}] ");
        }

        public static bool Split(string input, string pattern, out string upto, out string after)
        {
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                upto = input.Substring(0, match.Index + 1);
                after = input[(match.Index + 1)..];
                return true;
            }
            else
            {
                upto = null;
                after = input;
                return false;
            }
        }

        public static void GetSignatureElements(MethodBase info, out string name, out string arguments)
        {
            name = info.Name;

            if (info.DeclaringType != null)
                name = info.DeclaringType.Name + "." + name;

            IEnumerable<string> argumentTypes = info
                .GetParameters()
                .Select(x => x.ParameterType.Name);

            arguments = string.Join(", ", argumentTypes);
            arguments = "(" + arguments + ")";
        }

        private static void SplitMethodSignature(string input, out string name, out string arguments)
        {
            int argumentIndex = input.IndexOf('(');
            int genericIndex = input.IndexOf('<');

            int index = input.Length - 1;

            if (argumentIndex > -1)
                index = argumentIndex;

            if (genericIndex > -1 && genericIndex < argumentIndex)
                index = genericIndex;

            name = input.Substring(0, index);
            arguments = input[index..];
        }
    }
}
