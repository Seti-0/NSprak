using NSprak.Execution;
using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Messaging;
using NSprak.Exceptions;

namespace NSprak.Tests.Types
{
    public class Input : TestCommand
    {
        public string Text { get; }

        public override bool IsPreOp => true;

        public override string Description => $"in: '{Text}'";

        public Input(string text)
        {
            Text = text;
        }

        public override void Invoke(ExecutionContext context)
        {
            context.Computer.Screen?.SendInput(Text);
        }
    }
}
