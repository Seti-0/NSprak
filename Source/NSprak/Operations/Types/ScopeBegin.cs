using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class ScopeBegin : Op
    {
        public override string Name => "Begin Scope";

        public override string ShortName => "BSc";

        public override void Execute(ExecutionContext context)
        {
            context.Memory.BeginScope(inherit: true);
        }
    }
}
