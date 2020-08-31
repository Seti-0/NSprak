using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class ArrayElementGet : Op
    {
        public override string Name => "Array Element Get";

        public override string ShortName => "AGet";

        public override void Execute(ExecutionContext context)
        {
            SprakNumber index = context.Memory.PopValue<SprakNumber>();
            SprakArray array = context.Memory.PopValue<SprakArray>();

            Value result = array.Value[(int) index.Value];

            context.Memory.PushValue(result);
        }
    }
}
