using NSprak.Language.Values;
using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class Pass : Op
    {
        public override string Name => "Pass";

        public override void Execute(ExecutionContext context) {}
    }
}
