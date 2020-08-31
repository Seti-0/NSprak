using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;

namespace NSprak.Operations.Types
{
    public class VariableSet : Op
    {
        public string TargetName { get; }

        public override string Name => "Set";

        public override object RawParam => TargetName;

        public VariableSet(string targetName)
        {
            TargetName = targetName;
        }

        public override void Execute(ExecutionContext context)
        {
            Value value = context.Memory.PopValue<Value>();
            context.Memory.SetVariable(TargetName, value);
        }
    }
}
