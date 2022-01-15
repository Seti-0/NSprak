using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Tests.Types
{
    public class Input : TestCommand
    {
        public string Text { get; }

        public Input(string text)
        {
            Text = text;
        }
    }
}
