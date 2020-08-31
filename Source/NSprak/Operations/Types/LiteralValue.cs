using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;

namespace NSprak.Operations.Types
{
    public class LiteralValue : Op
    {
        public Value Value { get; }

        public override string Name => "Value";

        public override string ShortName => "Val";

        public override object RawParam => Value;

        public LiteralValue(Value value)
        {
            Value = value;
        }

        public override void Execute(ExecutionContext context)
        {
            context.Memory.PushValue(Value);
        }
    }
}
