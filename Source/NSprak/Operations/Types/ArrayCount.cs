using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class ArrayCount : Op
    {
        public string TargetName { get; }

        public override string Name => "Array Count";

        public override string ShortName => "ACount";

        public override object RawParam => TargetName;

        public ArrayCount(string targetName)
        {
            TargetName = targetName;
        }

        public override void Execute(ExecutionContext context)
        {
            SprakArray array = context.Memory.GetVariable<SprakArray>(TargetName);
            SprakNumber count = new SprakNumber(array.Value.Count);

            context.Memory.PushValue(count);
        }
    }
}
