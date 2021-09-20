using System;
using NSprak.Functions.Resolution;
using NSprak.Functions.Signatures;
using NSprak.Operations;

namespace NSprak.Functions.Resolution.OpBindings
{
    public class OperatorOpBinding : OperationBinding
    {
        public OperatorSignature Signature { get; }

        public OperatorOpBinding(Func<Op> builder, OperatorSignature signature) : base(builder)
        {
            Signature = signature;
        }
    }
}
