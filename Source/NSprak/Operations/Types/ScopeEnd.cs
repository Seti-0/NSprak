using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class ScopeEnd : Op
    {
        public override string Name => "End Scope";

        public override string ShortName => "ESc";

        public override void Execute(ExecutionContext context)
        {
            context.Memory.EndScope();
        }
    }
}
