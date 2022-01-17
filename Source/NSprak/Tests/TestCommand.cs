using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using NSprak.Tokens;
using NSprak.Messaging;
using NSprak.Execution;
using NSprak.Tests.Types;

namespace NSprak.Tests
{
    public abstract class TestCommand
    {
        private static List<string> errorNames;

        static TestCommand()
        {
            errorNames = typeof(Messages)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(MessageTemplate))
                .Select(field => field.Name)
                .ToList();
        }

        public static void ParseAndLinkToken(Token token, Messenger messages)
        {
            string text = token.Content[2..].Trim();

            if (text.StartsWith("test "))
            {
                if (!token.IsInitial())
                    messages.AtToken(token, Messages.UnexpectedTestTitle);

                // Just ignore this case - it's a test title,
                // used for organizational purposes
                // but not actually parsed into the expression tree.
                return;
            }

            TestCommand command = null;

            if (text.StartsWith("cerr "))
            {
                // This is a special case - a compile time assertion.
                // There is no need to create a TestCommand object and attach it to an expression,
                // the assertion can be made straight away.

                string content = text
                    .Substring("cerr ".Length)
                    .Trim();

                IList<string> words = content
                    .Split(' ')
                    .Select(x => x.Trim())
                    .ToArray();

                if (words.Count != 3)
                    messages.AtToken(token, Messages.TestParseError, content);

                else if (!errorNames.Contains(words[0]))
                    messages.AtToken(token, Messages.UnrecognizedError, words[0]);

                else if (!int.TryParse(words[1], out int line))
                    messages.AtToken(token, Messages.TestValueParseError, words[1]);

                else if (!int.TryParse(words[2], out int col))
                    messages.AtToken(token, Messages.TestValueParseError, words[2]);

                else
                {
                    string errorName = words[0];

                    foreach (Message message in messages.Messages)
                        if (message.Template.Name == errorName)
                        {
                            MessageLocation loc = message.Location;
                            // Note: line and col are one-based, while the message location is zero-based 
                            if (loc.LineStart == line - 1 && loc.ColumnStart == col - 1)
                            {
                                message.AccountedFor = true;
                                return;
                            }
                        }

                    {
                        string message = $"Expected message: '{errorName}' at {line}:{col}";
                        messages.AtToken(token, Messages.AssertionFailed, message);
                    }
                }
            }
            else if (text.StartsWith("err "))
            {
                string errorName = text.Substring("err ".Length).Trim();

                if (errorName.Length == 0)
                    messages.AtToken(token, Messages.MissingContent);

                else if (!errorNames.Contains(errorName))
                    messages.AtToken(token, Messages.UnrecognizedError, errorName);

                command = new ErrorTest(errorName);
            }
            else if (text.StartsWith("in "))
            {
                string content = text.Substring("in ".Length).Trim().Trim('"');
                command = new Input(content);
            }
            else if (text.StartsWith("out "))
            {
                string content = text.Substring("out ".Length).Trim().Trim('"');
                command = new Output(content);
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

                else 
                    command = new Equality(words[0], words[2]);
            }

            if (command == null)
                return;

            Token next = token.FindNextToken(token => token.Type != TokenType.Comment);
            if (next != null && next.ExpressionHint != null)
            {
                next.ExpressionHint.AddTest(command);
                return;
            }

            Token previous = token.FindPreviousToken(token => token.Type != TokenType.Comment);
            if (previous != null && previous.ExpressionHint != null)
            {
                previous.ExpressionHint.AddTest(command);
                return;
            }

            string description = command.GetType().Name.ToLower();
            messages.AtToken(token, Messages.ExpectedExpressionForCommand, description);
        }

        public abstract string Description { get; }

        // By default, test commands execute *after* the expression that follows them.
        // If this is true, it is executed before the expression.
        public virtual bool IsPreOp => false;

        public abstract void Invoke(ExecutionContext context);

        public override string ToString()
        {
            return Description;
        }
    }
}
