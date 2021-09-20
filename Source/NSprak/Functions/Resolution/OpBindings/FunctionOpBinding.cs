using System;
using NSprak.Functions.Resolution;
using NSprak.Functions.Signatures;
using NSprak.Operations;

namespace NSprak.Functions.Resolution.OpBindings
{
    public class FunctionOpBinding : OperationBinding
    {
        public FunctionSignature Signature { get; }

        public FunctionOpBinding(Func<Op> builder, FunctionSignature signature) : base(builder)
        {
            Signature = signature;
        }
    }
}
