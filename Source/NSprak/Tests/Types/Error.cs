using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Tests.Types
{
    public class Error : TestCommand
    {
        public string Name { get; }

        public Error(string name)
        {
            Name = name;
        }
    }
}
