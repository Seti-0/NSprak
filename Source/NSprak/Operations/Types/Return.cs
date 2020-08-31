using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class Return : Op
    {
        public override bool StepAfterwards => false;

        public override string Name => "Return";

        public override string ShortName => "Ret";

        public override void Execute(ExecutionContext context)
        {
            context.Return();
            context.Memory.EndScope();
        }
    }
}
