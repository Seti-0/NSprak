using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class Increment : Op
    {
        public string TargetName { get; }

        public override string Name => "Increment";

        public override string ShortName => "Inc";

        public override object RawParam => TargetName;

        public Increment(string targetName)
        {
            TargetName = targetName;
        }

        public override void Execute(ExecutionContext context)
        {
            SprakNumber input = context.Memory.GetVariable<SprakNumber>(TargetName);
            SprakNumber output = new SprakNumber(input.Value + 1);
            context.Memory.SetVariable(TargetName, output);
        }
    }
}
