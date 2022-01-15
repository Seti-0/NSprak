using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using NSprak.Tokens;
using NSprak.Messaging;

using NSprak.Tests.Types;

namespace NSprak.Tests
{
    public class TestCommand
    {
        private static List<string> errorNames;

        static TestCommand()
        {
            errorNames = typeof(Messages)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(Message))
                .Select(field => field.Name)
                .ToList();
        }

        public static TestCommand FromToken(Token token, Messenger messages)
        {
            string text = token.Content[2..].Trim();

            if (text.StartsWith("test "))
            {
                if (!token.IsInitial())
                    messages.AtToken(token, Messages.UnexpectedTestTitle);

                // Just ignore this case - it's a test title,
                // used for organizational purposes
                // but not actually parsed into the expression tree.
                return null;
            }
            else if (text.StartsWith("err "))
            {
                string errorName = text.Substring("err ".Length).Trim();

                if (errorName.Length == 0)
                    messages.AtToken(token, Messages.MissingContent);

                if (!errorNames.Contains(errorName))
                    messages.AtToken(token, Messages.UnrecognizedError, errorName);

                else
                    return new Error(errorName);
            }
            else if (text.StartsWith("in "))
            {
                string content = text.Substring("in ".Length).Trim().Trim('"');
                return new Input(content);
            }
            else if (text.StartsWith("out "))
            {
                string content = text.Substring("out ".Length).Trim().Trim('"');
                return new Output(content);
            }
            else
            {
                string[] words = text.Split(' ')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToArray();

                if (words.Length != 3 || words[1] != "eq")
                    messages.AtToken(token, Messages.TestParseError, text);

                else if (!Equality.IsValidValue(words[0]))
                    messages.AtToken(token, Messages.TestValueParseError, words[0]);

                else if (!Equality.IsValidValue(words[1]))
                    messages.AtToken(token, Messages.TestValueParseError, words[2]);

                else return new Equality(words[0], words[2]);
            }

            return null;
        }
    }
}
