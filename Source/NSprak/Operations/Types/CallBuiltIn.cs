using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Exceptions;
using NSprak.Execution;
using NSprak.Functions;
using NSprak.Language;

namespace NSprak.Operations.Types
{
    public class CallBuiltIn : Op
    {
        public BuiltInFunction Function { get; }

        public override string Name => "Call Builtin";

        public override string ShortName => "CallB";

        public override object RawParam => Function;

        public CallBuiltIn(BuiltInFunction function)
        {
            Function = function;
        }

        public override void Execute(ExecutionContext context)
        {
            List<Value> values = new List<Value>();

            foreach (SprakType type in Function.Signature.TypeSignature.Parameters.Reverse())
                values.Add(context.Memory.PopValue(type));

            // Reverse is important here - they are variables off a stack
            values.Reverse();

            Value result = Function.Call(values.ToArray(), context);

            if (Function.ReturnType != SprakType.Unit)
                context.Memory.PushValue(result);
        }
    }
}
