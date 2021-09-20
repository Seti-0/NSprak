using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Language.Values;

namespace NSprak.Operations.Types
{
    public class ArrayElementSet : Op
    {
        public string VariableName { get; }

        public override string Name => "Array Element Set";

        public override string ShortName => "ASet";

        public override object RawParam => VariableName;

        public ArrayElementSet(string name)
        {
            VariableName = name;
        }

        public override void Execute(ExecutionContext context)
        {
            SprakNumber index = context.Memory.PopValue<SprakNumber>();
            Value value = context.Memory.PopValue();

            SprakArray array = context.Memory
                .GetVariable<SprakArray>(VariableName);

            array.Value[(int)index.Value] = value;
        }
    }
}
