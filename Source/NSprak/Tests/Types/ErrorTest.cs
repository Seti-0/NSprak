using NSprak.Execution;
using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Messaging;
using NSprak.Exceptions;

namespace NSprak.Tests.Types
{
    public class ErrorTest : TestCommand
    {
        public string ErrorName { get; }

        public override string Description => $" err: '{ErrorName}'";

        public ErrorTest(string name)
        {
            ErrorName = name;
        }

        public override void Invoke(ExecutionContext context)
        {
            // Test commands are only invoke this way when the operation succeeds , which means
            // this has automatically failed. If the operation fails, the executor will check for an assertion
            // like this and allow the test to pass.
            string message = $"Expected error: {ErrorName}. No error occurred.";
            throw new SprakRuntimeException(Messages.AssertionFailed, message);
        }
    }
}
