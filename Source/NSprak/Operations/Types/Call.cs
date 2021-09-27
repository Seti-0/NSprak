using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Exceptions;
using NSprak.Execution;
using NSprak.Functions.Signatures;

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

            // inherit true here means that functions can access the global scope
            context.Memory.BeginScope(inherit: true);
            context.BeginFrame(index, Signature);
        }
    }
}
