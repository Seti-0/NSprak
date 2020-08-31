using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class JumpLabelConditionalNegated : Op
    {
        public string LabelName { get; }

        public override string Name => "Negated Conditional Label Jump";

        public override string ShortName => "CLJmp.N";

        public override object RawParam => LabelName;

        public override bool StepAfterwards => false;

        public JumpLabelConditionalNegated(string label)
        {
            LabelName = label;
        }

        public override void Execute(ExecutionContext context)
        {
            SprakBoolean condition = context.Memory.PopValue<SprakBoolean>();

            if (!condition.Value)
                context.Instructions.Jump(LabelName);
            else
                context.Instructions.Step();
        }
    }
}
