using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations.Types
{
    public class JumpLabel : Op
    {
        public string LabelName { get; }

        public override string Name => "Label Jump";

        public override string ShortName => "LJmp";

        public override object RawParam => LabelName;

        public override bool StepAfterwards => false;

        public JumpLabel(string label)
        {
            LabelName = label;
        }

        public override void Execute(ExecutionContext context)
        {
            context.Instructions.Jump(LabelName);
        }
    }
}
