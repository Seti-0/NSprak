using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Exceptions;
using NSprak.Execution;
using NSprak.Language.Builtins;

namespace NSprak.Operations.Types
{
    public class Call : Op
    {
        public FunctionSignature Signature { get; }

        public override string Name => "Call";

        public override object RawParam => Signature;

        public override bool StepAfterwards => false;

        public Call(FunctionSignature signature)
        {
            Signature = signature;
        }

        public override void Execute(ExecutionContext context)
        {
            if (!context.Executable.EntryPoints.TryGetValue(Signature, out int index))
            {
                string message = $"Unable to begin function with signture {Signature}";
                throw new SprakInternalExecutionException(message);
            }

            context.Memory.BeginScope(inherit: false);
            context.BeginFrame(index, Signature);
        }
    }
}
