using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class Exit : Op
    {
        public override string Name => "Exit";

        public override void Execute(ExecutionContext context)
        {
            context.RequestExit();
        }
    }
}
