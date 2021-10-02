using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Messaging;

namespace NSprak
{
    public class Computer
    {
        public Compiler Compiler { get; } = new Compiler();

        public Executable Executable { get; private set; } = new Executable();

        public IComputerScreen Screen { get; set; }

        public Messenger Messenger { get; set; } = new Messenger();

        public string Source { get; set; }

        public bool Compile()
        {
            Messenger.Clear();

            Executable exe = Compiler.Compile(Source, Messenger);
            
            if (exe == null)
                return false;

            else
            {
                Executable = exe;
                return true;
            }
        }

        public Executor CreateExecutor()
        {
            return new Executor(this);
        }
    }
}
