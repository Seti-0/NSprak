using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Language;
using NSprak.Execution;
using NSprak.Messaging;
using NSprak.Exceptions;

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

        public override string Description => $"'{Left}' eq '{Right}'";

        public Equality(string left, string right)
        {
            Left = left;
            Right = right;
        }

        public override void Invoke(ExecutionContext context)
        {
            // Each side is either a literal value, or a variable name.

            Value leftValue;
            if (!Value.TryParse(Left, out leftValue))
                if (!context.Memory.TryGetVariable(Left, out leftValue))
                {
                    string message = $"Unable to read: '{Left}'";
                    throw new SprakRuntimeException(Messages.AssertionExecutionFailed, message);
                }

            Value rightValue;
            if (!Value.TryParse(Right, out rightValue))
                if (!context.Memory.TryGetVariable(Right, out rightValue))
                {
                    string message = $"Unable to read: '{Right}'";
                    throw new SprakRuntimeException(Messages.AssertionExecutionFailed, message);
                }

            if (leftValue != rightValue)
            {
                string message = $"LHS: {leftValue}, RHS: {rightValue}";
                throw new SprakRuntimeException(Messages.AssertionFailed, message);
            }
        }
    }
}
