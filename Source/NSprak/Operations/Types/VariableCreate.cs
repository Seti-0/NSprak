using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;

namespace NSprak.Operations.Types
{
    public class VariableCreate : Op
    {
        public string NewVariableName { get; }

        public override string Name => "Declare";

        public override string ShortName => "Dec";

        public override object RawParam => NewVariableName;

        public VariableCreate(string name)
        {
            NewVariableName = name;
        }

        public override void Execute(ExecutionContext context)
        {
            Value initialValue = context.Memory.PopValue();
            context.Memory.Declare(NewVariableName, initialValue);
        }
    }
}
