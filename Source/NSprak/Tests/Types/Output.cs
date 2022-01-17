using NSprak.Execution;
using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Messaging;
using NSprak.Exceptions;

namespace NSprak.Tests.Types
{
    public class Output : TestCommand
    {
        public string Expected { get; }

        public override string Description => $"out: '{Expected}'";

        public Output(string text)
        {
            Expected = text;
        }
        public override void Invoke(ExecutionContext context)
        {
            string Found = context.Computer.Screen?.RetrieveOutput();
            if (Found != Expected)
            {
                string details = $"Expected: '{Expected}', Found: '{Found}'";
                throw new SprakRuntimeException(Messages.AssertionFailed, details);
            }
        }
    }
}
