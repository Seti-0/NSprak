using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;

namespace NSprak.Operations.Types
{
    public class VariableGet : Op
    {
        public string TargetName { get; }

        public override string Name => "Get";

        public override object RawParam => TargetName;

        public VariableGet(string targetName)
        {
            TargetName = targetName;
        }

        public override void Execute(ExecutionContext context)
        {
            Value value = context.Memory.GetVariable(TargetName);
            context.Memory.PushValue(value);
        }
    }
}
