using System;
using NSprak.Operations;

namespace NSprak.Functions.Resolution
{
    public abstract class OperationBinding
    {
        public Func<Op> Builder { get; }

        public OperationBinding(Func<Op> builder)
        {
            Builder = builder;
        }
    }
}
