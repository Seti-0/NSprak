using System;
using NSprak.Functions.Resolution;
using NSprak.Functions.Signatures;
using NSprak.Operations;

namespace NSprak.Functions.Resolution.OpBindings
{
    public class ConversionOpBinding : OperationBinding
    {
        public ConversionTypeSignature Signature { get; }

        public ConversionOpBinding(Func<Op> builder, ConversionTypeSignature signature) : base(builder)
        {
            Signature = signature;
        }
    }
}
