using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class ArrayValue : Op
    {
        public int Count { get; }

        public override string Name => "Array Value";

        public override string ShortName => "AVal";

        public override object RawParam => Count;

        public ArrayValue(int count)
        {
            Count = count;
        }

        public override void Execute(ExecutionContext context)
        {
            List<Value> values = new List<Value>();

            for (int i = 0; i < Count; i++)
                values.Add(context.Memory.PopValue<Value>());

            values.Reverse();

            context.Memory.PushValue(new SprakArray(values));
        }
    }
}
