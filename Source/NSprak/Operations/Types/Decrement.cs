using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class Decrement : Op
    {
        public string TargetName { get; }

        public override string Name => "Decrement";

        public override string ShortName => "Dec";

        public override object RawParam => TargetName;

        public Decrement(string targetName)
        {
            TargetName = targetName;
        }

        public override void Execute(ExecutionContext context)
        {
            SprakNumber input = context.Memory.GetVariable<SprakNumber>(TargetName);
            SprakNumber output = new SprakNumber(input.Value - 1);
            context.Memory.SetVariable(TargetName, output);
        }
    }
}
