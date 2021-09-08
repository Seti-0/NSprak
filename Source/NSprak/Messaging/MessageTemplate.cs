using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace NSprak.Messaging
{
    public class MessageTemplate
    {
        public MessageSeverity Severity { get; set; }

        public bool IsError => Severity == MessageSeverity.Error;

        public string Summary { get; set; }

        public override string ToString()
        {
            string severity = Enum.GetName(typeof(MessageSeverity), Severity);
            string result = $"[{severity}] {Summary}";
            return result;
        }

        public string Render(IList<object> parameters)
        {
            // There is definitely a built-in way to do this, but as
            // of writing this I did not have the internet access with
            // which to check.

            MatchCollection matches = Regex.Matches(Summary, @"\{[^\}]+\}");

            int paramCount = parameters?.Count ?? 0;

            if (matches.Count != paramCount)
            {
                string message = $"Expected {matches.Count} " +
                    $"parameters for template: '{Summary}'," +
                    $" found {parameters.Count}";

                throw new ArgumentException(message);
            }

            string result = "";
            int startIndex = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // I'd be surprised if there was not a built in utility for
                // this as well somewhere
                result += Summary.Substring(startIndex, matches[i].Index);
                result += parameters[i].ToString();

                startIndex = matches[i].Index + matches[i].Length;
            }

            result += Summary.Substring(startIndex, Summary.Length - startIndex);

            return result;
        }
    }
}
