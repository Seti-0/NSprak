using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class GreaterThan : Op
    {
        public override string Name => "Greater Than";

        public override string ShortName => "Gt";

        public override void Execute(ExecutionContext context)
        {
            // Remember, latest first on a stack
            SprakNumber second = context.Memory.PopValue<SprakNumber>();
            SprakNumber first = context.Memory.PopValue<SprakNumber>();

            SprakBoolean result = new SprakBoolean(first.Value > second.Value);

            context.Memory.PushValue(result);
        }
    }
}
